using System;

namespace SignalHelper.Models;
internal class EnrichedSignal : OhlcSignal {
    public EnrichedSignal(double open, double high, double low, double close, double volume, DateTime date) : base(open, high, low, close, volume, date) { }

    public double Fit1 { get; set; }
    public double Fit2 { get; set; }

    public double FitRsquared1 { get; set; }
    public double FitRsquared2 { get; set; }

    public double Rsi { get; set; }
    public double Mfi { get; set; }
    public double StochasticK { get; set; }
    public double StochasticD { get; set; }

    public double AttachedRsi { get; set; }
    public double AttachedMfi { get; set; }
    public bool IsBuy { get; set; }
    public bool IsSell { get; set; }
}