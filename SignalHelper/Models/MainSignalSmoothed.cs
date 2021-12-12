using System;

namespace SignalHelper.Models {
    internal class MainSignalSmoothed {
        internal MainSignalSmoothed(DateTime date, double smoothed, double derivativeSmoothed) => (Date, Smoothed, DerivativeSmoothed) = (date, smoothed, derivativeSmoothed);

        internal DateTime Date { get; private set; }
        internal double Smoothed { get; private set; }
        internal double DerivativeSmoothed { get; private set; }
    }
}
