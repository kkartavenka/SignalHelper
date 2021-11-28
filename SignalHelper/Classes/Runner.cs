using ScottPlot;
using SignalHelper.Classes.Indicators;
using SignalHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SignalHelper.Classes;
internal class Runner {

    private int _period;
    private int _clickedIndex;
    private int _plotLastSelectedIndex;
    private readonly List<EnrichedSignal>? _signal;
    private List<EnrichedSignal>? _attachedSignal;

    internal Runner(string filename) {

        SignalName = filename.Split('\\').Last().Split('.').First();

        var csv = new CsvReader(filename, ',', new DateTime(0));
        csv.PrepareSourceData(true);

        _signal = new(csv.Ohlc.Count);
        csv.Ohlc.ForEach(row => _signal.Add(new(open: row.Open, high: row.High, low: row.Low, close: row.Close, volume: row.Volume, date: row.Date)));
        Plot = Signal.Select(m => new OHLC(open: m.Open, high: m.High, low: m.Low, close: m.Close, timeStart: m.Date.ToOADate())).ToList();

        Dates = Signal.Select(m => m.Date.ToOADate()).ToArray();

        new RelativeStrengthIndex(period: 14).Enrich(ref _signal);
        new MoneyFlowIndex(period: 14).Enrich(ref _signal);

        SelectedCandle[0] = Plot.ElementAt(0);
    }

    internal void AttachSignal(string filename, int rsiPeriod = 14, int mfiPeriod = 14) {
        if (_signal == null) return;

        AttachedSignalName = filename.Split('\\').Last().Split('.').First();

        var csv = new CsvReader(filename, ',', new DateTime(0));
        csv.PrepareSourceData(true);

        _attachedSignal = new(csv.Ohlc.Count);
        csv.Ohlc.ForEach(row => _attachedSignal.Add(new(open: row.Open, high: row.High, low: row.Low, close: row.Close, volume: row.Volume, date: row.Date)));

        new RelativeStrengthIndex(rsiPeriod).Enrich(ref _attachedSignal);
        new MoneyFlowIndex(mfiPeriod).Enrich(ref _attachedSignal);

        _period = Math.Max(rsiPeriod, mfiPeriod);

        bool findMatch = false;
        _signal.ForEach(row => {
            findMatch = false;

            for (int i = 1; i < _attachedSignal.Count; i++)
                if (_attachedSignal[i].Date >= row.Date && _attachedSignal[i - 1].Date <= row.Date) {
                    if (_attachedSignal[i].Date - row.Date > row.Date - _attachedSignal[i - 1].Date) {
                        row.AttachedRsi = _attachedSignal[i - 1].Rsi;
                        row.AttachedMfi = _attachedSignal[i - 1].Mfi;
                    } else {
                        row.AttachedRsi = _attachedSignal[i].Rsi;
                        row.AttachedMfi = _attachedSignal[i].Mfi;
                    }

                    findMatch = true;
                }

            if (!findMatch) {
                throw new Exception("Not matched!");
            }
        });
    }

    internal void ClearPositions() => Signal.ForEach(row => {
        row.IsBuy = false;
        row.IsSell = false;
    });

    internal bool ExportToCsv(string filename, string signalType) {
        if (_signal == null)
            return false;

        string header = "signal_name,signal_type,attached_signal_name,is_buy,is_sell,date,open,high,low,close,volume,signal_rsi,signal_mfi,attached_signal_rsi,attached_signal_mfi";
        
        var lines = new List<string>(_signal.Count-_period);

        lines.AddRange(_signal.Skip(_period + 2).Select(m => $"{SignalName},{signalType},{AttachedSignalName},{m.IsBuy},{m.IsSell},{m.Date.ToOADate()},{m.Open},{m.High},{m.Low},{m.Close},{m.Volume},{m.Rsi},{m.Mfi},{m.AttachedRsi},{m.AttachedMfi}"));

        if (!File.Exists(filename)) {
            lines.Insert(0, header);
            File.WriteAllLines(filename, lines);
        } else
            File.AppendAllLines(filename, lines);

        return true;
    }

    internal bool GetNearbyCandle(double x) {
        int nearbyIndex = GetNearbyCandleIndex(x);

        if (nearbyIndex == 0 || _plotLastSelectedIndex == nearbyIndex)
            return false;

        _plotLastSelectedIndex = nearbyIndex;
        SelectedCandle[0] = Plot[_plotLastSelectedIndex];
        return true;
    }

    private int GetNearbyCandleIndex(double x) {
        int secondIndex = Array.FindIndex(Dates, m => m > x);
        int firstIndex = secondIndex > 0 ? secondIndex - 1 : 0;

        if (secondIndex < 0 || firstIndex == secondIndex)
            return 0;

        if (secondIndex - x < x - firstIndex)
            return secondIndex;
        else
            return firstIndex;
    }

    internal void SetClickedIndex(double x) => _clickedIndex = GetNearbyCandleIndex(x);

    internal void SetBuy(int index = -1) {
        if (_signal == null) return;

        if (index > -1) {
            _signal[index].IsBuy = true;
            return;
        }

        _signal[_clickedIndex].IsBuy = !_signal[_clickedIndex].IsBuy;
    }

    internal void SetSell(int index = -1) {
        if (_signal == null) return;

        if (index > -1) {
            _signal[index].IsSell = true;
            return;
        }

        _signal[_clickedIndex].IsSell = !_signal[_clickedIndex].IsSell;
    }

    #region Properties
    internal string AttachedSignalName { get; private set; } = string.Empty;
    private double[] Dates { get; }
    internal List<OHLC> Plot { get; }
    internal OHLC[] SelectedCandle { get; } = new OHLC[1];
    internal List<EnrichedSignal> Signal { get => _signal ?? new List<EnrichedSignal>(); }
    internal string SignalName { get; private set; }
    #endregion
}