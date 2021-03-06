using Accord.Statistics;
using SignalHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SignalHelper.Classes;

internal class CsvReader {
    internal enum MetaTraderColumn : int { Date = 0, Time = 1, Open = 2, High = 3, Low = 4, Close = 5, Volume = 6 };

    internal CsvReader(string fileName, char splitChar, DateTime restrictDate) {
        string[] lines = File.ReadAllLines(fileName);

        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            string[] row = line.Split(splitChar);

            DateTime trueDate;

            string dateTimeString = $"{row[(int)MetaTraderColumn.Date]} {row[(int)MetaTraderColumn.Time]}";
            bool v1Parsed = DateTime.TryParseExact(dateTimeString, "yyyy.MM.dd H:mm", null, System.Globalization.DateTimeStyles.None, out DateTime v1);
            bool v2Parsed = DateTime.TryParseExact(dateTimeString, "yyyy.MM.dd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime v2);

            trueDate = v1Parsed ? v1 : v2Parsed ? v2 : new();

            if (trueDate != new DateTime()) {
                OhlcSignal newElement = new(
                    open: row.ConvertTo<double>(MetaTraderColumn.Open),
                    high: row.ConvertTo<double>(MetaTraderColumn.High),
                    low: row.ConvertTo<double>(MetaTraderColumn.Low),
                    close: row.ConvertTo<double>(MetaTraderColumn.Close),
                    volume: row.ConvertTo<double>(MetaTraderColumn.Volume),
                    date: trueDate);

                Ohlc.Add(newElement);
                try {
                    int dotIndex = row[(int)MetaTraderColumn.Close].IndexOf('.');
                    int roundPoint = row[(int)MetaTraderColumn.Close].Length - dotIndex;
                    if (dotIndex > -1 && roundPoint > RoundPoint)
                        RoundPoint = roundPoint;
                } catch (Exception error) {
                    Console.WriteLine($"Exception in parsing separation sign: {error.Message}");
                }
            } else
                Console.WriteLine("Error parsing date");
        }

        Ohlc = Ohlc
            .OrderBy(m => m.Date)
            .Where(m => m.Date >= restrictDate)
            .ToList();

        RoundPoint--;
    }

    internal void PrepareSourceData(bool cleanUp, int cleanUpWindowSize = 20) {
        int i = cleanUpWindowSize;
        while (cleanUp && i < Ohlc.Count) {
            List<OhlcSignal> selectedSequence = Ohlc.Skip(i - cleanUpWindowSize).Take(cleanUpWindowSize).ToList();
            double median = selectedSequence.Select(m => (double)m.Volume).SkipLast(1).ToArray().Median();

            if (Ohlc[i - 1].Volume <= median * 0.15) {
                if (i > 2 && i < Ohlc.Count) {
                    int dayBefore = (int)Math.Abs(Math.Round((Ohlc[i - 2].Date - Ohlc[i - 1].Date).TotalDays));
                    int dayAfter = (int)Math.Abs(Math.Round((Ohlc[i].Date - Ohlc[i - 1].Date).TotalDays));

                    if (dayAfter >= 2 && dayBefore == 1) {
                        Ohlc[i - 2].Volume += Ohlc[i - 1].Volume;
                        Ohlc[i - 2].Close = Ohlc[i - 1].Close;
                        Ohlc[i - 2].High = Math.Max(Ohlc[i - 1].High, Ohlc[i - 2].High);
                        Ohlc[i - 2].Low = Math.Min(Ohlc[i - 1].Low, Ohlc[i - 2].Low);
                        Ohlc.RemoveAt(i - 1);
                    } else if (dayBefore >= 2 && dayAfter == 1) {
                        Ohlc[i].Volume += Ohlc[i - 1].Volume;
                        Ohlc[i].Open = Ohlc[i - 1].Open;
                        Ohlc[i].High = Math.Max(Ohlc[i - 1].High, Ohlc[i].High);
                        Ohlc[i].Low = Math.Min(Ohlc[i - 1].Low, Ohlc[i].Low);

                        Ohlc.RemoveAt(i - 1);
                    } else if (Ohlc[i - 1].Date.Day == 1 && Ohlc[i - 1].Date.Month == 1)
                        Ohlc.RemoveAt(i - 1);
                    else
                        i++;
                }
            } else
                i++;
        }
    }

    #region Properties

    internal int DataSize { get; private set; }
    internal List<OhlcSignal> Ohlc { get; private set; } = new();

    internal int RoundPoint { get; private set; }

    #endregion

}