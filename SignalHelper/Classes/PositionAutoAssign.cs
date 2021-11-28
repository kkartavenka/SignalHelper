using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using System.Linq;

namespace SignalHelper.Classes {
    internal class PositionAutoAssign {
        private readonly double _minRSquared, _minMeanChange;
        private readonly int _period;

        internal PositionAutoAssign(int period, double minMeanChange, double minRSquared) => (_period, _minMeanChange, _minRSquared) = (period, minMeanChange, minRSquared);
        internal void AssignViaTypicalPrice(ref Runner runner) {
            runner.ClearPositions();

            var x = Enumerable.Range(0, _period).Select(m => (double)m).ToArray();

            for (int i = 0; i < runner.Signal.Count; i++) {
                var yObserved = runner.Signal.Skip(i).Take(_period).Select(m => m.TypicalPrice).ToArray();

                var ols = new OrdinaryLeastSquares();
                var regression = ols.Learn(x, yObserved);

                var yExpected = regression.Transform(x);
                var rSquared = new RSquaredLoss(1, yObserved).Loss(yExpected);

                if (rSquared < _minRSquared)
                    continue;

                if ((regression.Slope > 0) && (yObserved.Max() / yObserved[0] / x.Length - 1) > _minMeanChange)
                    runner.SetBuy(i);
                else if ((regression.Slope < 0) && (yObserved[0] / yObserved.Min() / x.Length - 1) > _minMeanChange)
                    runner.SetSell(i);
            }
        }
    }
}
