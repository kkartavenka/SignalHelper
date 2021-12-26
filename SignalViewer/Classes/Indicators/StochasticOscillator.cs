using Accord.Statistics;
using SignalHelper.Models;
using System.Collections.Generic;
using System.Linq;

namespace SignalHelper.Classes.Indicators;

internal class StochasticOscillator {
    private readonly int _k, _d;
    
    internal StochasticOscillator(int k, int d) => (_k, _d) = (k, d);

    internal void Enrich(ref List<EnrichedSignal> signal) {
        for (int i = _k; i < signal.Count + 1; i++) {
            var selected = signal.Skip(i - _k).Take(_k);
            var highest = selected.Max(m => m.High);
            var lowest = selected.Min(m => m.Low);
            signal[i - 1].StochasticK = (signal[i - 1].Close - lowest) / (highest - lowest);
            signal[i - 1].StochasticD = selected.TakeLast(_d).Select(m => m.StochasticK).ToArray().Mean();
        }
    }
}