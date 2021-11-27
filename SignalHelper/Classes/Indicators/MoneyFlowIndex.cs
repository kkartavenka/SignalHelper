using SignalHelper.Models;
using System;
using System.Collections.Generic;

namespace SignalHelper.Classes.Indicators;

internal class MoneyFlowIndex {
    private readonly int _period;
    internal MoneyFlowIndex(int period) => _period = period;

    internal void Enrich(ref List<EnrichedSignal> signal) {
        double[] moneyFlow = new double[signal.Count - 1];

        for (int i = 1; i < signal.Count; i++)
            moneyFlow[i - 1] = (signal[i].TypicalPrice > signal[i - 1].TypicalPrice ? 1 : -1) * signal[i].TypicalPrice * signal[i].Volume;

        for (int i = moneyFlow.Length; i > _period + 1; i--) {
            double[] sequence = moneyFlow[(i - _period)..i];

            double positiveMoneyFlow = 0;
            double negativeMoneyFlow = 0;

            for (int j = 0; j < sequence.Length; j++)
                if (sequence[j] > 0)
                    positiveMoneyFlow += sequence[j];
                else
                    negativeMoneyFlow += Math.Abs(sequence[j]);

            double mfi = 1;

            if (negativeMoneyFlow != 0)
                mfi = 1 - 1 / (1 + positiveMoneyFlow / negativeMoneyFlow);

            signal[i].Mfi = mfi;
        }

    }
}
