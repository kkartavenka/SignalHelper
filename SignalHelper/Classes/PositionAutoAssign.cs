using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using SignalHelper.Classes.Filters;
using System.Linq;

namespace SignalHelper.Classes {
    internal class PositionAutoAssign {
        private readonly double _minRSquared, _minMeanChange;
        private readonly int _period;
        private const int _shift = 2;

        internal PositionAutoAssign(int period, double minMeanChange, double minRSquared) => (_period, _minMeanChange, _minRSquared) = (period, minMeanChange, minRSquared);
        internal void AssignViaTypicalPrice(ref Runner runner) {
            runner.ClearPositions();

            var x = Enumerable.Range(0, _period).Select(m => (double)m).ToArray();
            var xReduced = Enumerable.Range(0, _period - _shift).Select(m => (double)m).ToArray();

            var sgFilter = new SavitzkyGolayFilter(windowSize: 11, polyOrder: 2);
            var unfiltered = runner.Signal.Select(m => m.Close).ToList();
            var filtered = sgFilter.ProcessSequence(unfiltered);

            for (int i = 0; i < runner.Signal.Count - _period; i++) {

                var yObservedReal = unfiltered.Skip(i).Take(_period).ToArray();

                var yObserved = filtered.Skip(i).Take(_period).ToArray();
                var yObservedReduced = filtered.Skip(i + _shift).Take(_period - _shift).ToArray();

                var ols = new OrdinaryLeastSquares();
                var regression = ols.Learn(x, yObserved);
                var regressionReduced = ols.Learn(xReduced, yObservedReduced);

                if (regression.Slope * regressionReduced.Slope < 0)
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
    }
}
