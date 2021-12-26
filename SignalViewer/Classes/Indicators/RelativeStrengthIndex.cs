using SignalHelper.Models;
using System.Collections.Generic;

namespace SignalHelper.Classes.Indicators;
internal class RelativeStrengthIndex {

    private readonly int _period;
    internal RelativeStrengthIndex(int period) => _period = period;

    internal void Enrich(ref List<EnrichedSignal> signal) {
        double[] derivative = new double[signal.Count - 1];

        for (int i = 1; i < signal.Count; i++)
            derivative[i - 1] = signal[i].Close - signal[i - 1].Close;

        for (int i = derivative.Length; i > _period + 1; i--) {
            double[] sequence = derivative[(i - _period)..i];

            double gain = 0, loss = 0;

            for (int j = 0; j < sequence.Length; j++)
                if (sequence[j] > 0)
                    gain += sequence[j];
                else
                    loss += (-1) * sequence[j];

            gain /= _period;
            loss /= _period;

            signal[i].Rsi = 1 - (1 / (1 + gain / loss));
        }
    }
}