using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using SignalHelper.Classes.Filters;
using SignalHelper.Models;
using System.Collections.Generic;
using System.Linq;

namespace SignalHelper.Classes;

internal class PositionAutoAssign {
    private readonly double _minRSquared, _minMeanChange;
    private readonly int _period;
    private const double _shift = 1 / 3;

    private readonly int _windowSize = 11, _polyOrder = 2;

    internal PositionAutoAssign(int period, double minMeanChange, double minRSquared, int windowSize, int polyOrder) => (_period, _minMeanChange, _minRSquared, _windowSize, _polyOrder) = (period, minMeanChange, minRSquared, windowSize, polyOrder);
    internal void AssignViaTypicalPrice(ref Runner runner) {
        runner.ClearPositions();

        var confirmationSize = (int)(_period * _shift);
        if (confirmationSize < 3)
            confirmationSize = 3;

        var x = Enumerable.Range(0, _period).Select(m => (double)m).ToArray();
        var xReduced = Enumerable.Range(0, confirmationSize).Select(m => (double)m).ToArray();

        var unfiltered = runner.Signal.Select(m => m.Close).ToList();
        var filtered = new SavitzkyGolayFilter(windowSize: _windowSize, polyOrder: _polyOrder).ProcessSequence(unfiltered);
        var filteredDerivative = new SavitzkyGolayFilter(windowSize: _windowSize, polyOrder: _polyOrder, derivativeOrder: 1).ProcessSequence(unfiltered);

        SmoothedSignal = new(filtered.Count);
        for (int i = 0; i < filtered.Count; i++)
            SmoothedSignal.Add(new(date: runner.Signal[i].Date, smoothed: filtered[i], derivativeSmoothed: filteredDerivative[i]));

        for (int i = 1; i < runner.Signal.Count - _period; i++) {

            if (filteredDerivative[i] * filteredDerivative[i - 1] > 0)
                continue;

            var yObservedReal = unfiltered.Skip(i).Take(_period).ToArray();

            var yObserved = filtered.Skip(i).Take(_period).ToArray();
            var yObservedReducedFirst = yObserved.Take(confirmationSize).ToArray();
            var yObservedReducedLast = yObserved.TakeLast(confirmationSize).ToArray();

            var ols = new OrdinaryLeastSquares();
            var regression = ols.Learn(x, yObserved);
            var regressionReducedFirst = ols.Learn(xReduced, yObservedReducedFirst);

            var regressionReducedLast = ols.Learn(xReduced, yObservedReducedLast);

            if ((regression.Slope * regressionReducedFirst.Slope < 0) || (regression.Slope * regressionReducedLast.Slope < 0))
                continue;

            var rSquaredFirst = new RSquaredLoss(1, yObservedReducedFirst).Loss(regressionReducedFirst.Transform(xReduced));
            var rSquaredLast = new RSquaredLoss(1, yObservedReducedLast).Loss(regressionReducedLast.Transform(xReduced));

            if (rSquaredFirst < _minMeanChange * 0.67 || rSquaredLast < _minMeanChange * 0.8)
                continue;

            var yExpected = regression.Transform(x);
            var rSquared = new RSquaredLoss(1, yObserved).Loss(yExpected);

            if (rSquared < _minRSquared)
                continue;

            if ((regression.Slope > 0) && ((yObservedReal.Max() / yObservedReal[0] - 1) / _period) > _minMeanChange)
                runner.SetBuy(i);
            else if ((regression.Slope < 0) && ((yObservedReal[0] / yObservedReal.Min() - 1) / x.Length) > _minMeanChange)
                runner.SetSell(i);
        }
    }

    internal List<MainSignalSmoothed> SmoothedSignal { get; private set; } = new();
}