using System;
using static SignalViewer.Classes.CsvReader;

namespace SignalViewer.Classes.Extensions;

internal static class StringExtension {
    internal static T ConvertTo<T>(this string[] values, MetaTraderColumn column) => (T)Convert.ChangeType(values[(int)column], typeof(T));
}