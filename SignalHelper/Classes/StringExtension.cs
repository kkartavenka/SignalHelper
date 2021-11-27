using System;
using static SignalHelper.Classes.CsvReader;

namespace SignalHelper.Classes;

internal static class StringExtension {
    public static T ConvertTo<T>(this string[] values, MetaTraderColumn column) => (T)Convert.ChangeType(values[(int)column], typeof(T));
}