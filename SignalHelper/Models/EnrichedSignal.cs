using System;

namespace SignalHelper.Models;
internal class EnrichedSignal : OhlcSignal {
    public EnrichedSignal(double open, double high, double low, double close, double volume, DateTime date) : base(open, high, low, close, volume, date) { }

    public double ClosePriceSlopeRange1 { get; set; }
    public double ClosePriceSlopeRange2 { get; set; }

    public double ClosePriceSlopeRange1RSquared { get; set; }
    public double ClosePriceSlopeRange2RSquared { get; set; }

    public double Rsi { get; set; }
    public double Mfi { get; set; }

    public double AttachedRsi { get; set; }
    public double AttachedMfi { get; set; }
    public bool IsBuy { get; set; }
    public bool IsSell { get; set; }
}