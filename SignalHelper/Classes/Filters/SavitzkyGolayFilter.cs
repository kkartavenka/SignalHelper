using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Models.Regression.Linear;
using SignalHelper.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalHelper.Classes.Filters;

public class SavitzkyGolayFilter {
    private readonly int _derivativeOrder;
    private readonly int _polyOrder;
    private readonly int _windowSize;

    private readonly double[] _coef;

    public SavitzkyGolayFilter(int windowSize, int polyOrder, int derivativeOrder = 0) {
        _windowSize = windowSize;
        _polyOrder = polyOrder;
        _derivativeOrder = derivativeOrder;

        if (windowSize < 3 || windowSize % 2 != 1)
            throw new Exception("Wrong parameters");
        else
            _coef = this.Coefficients();
    }

    private double[] Coefficients() {
        double[] coef = new double[_windowSize];

        if (_derivativeOrder > _polyOrder)
            return coef;

        int pos = _windowSize / 2;

        for (int i = 0; i < _windowSize; i++)
            coef[i] = pos - i;

        double[] matrix2 = new double[_polyOrder + 1];
        for (int i = 0; i < matrix2.Length; i++)
            matrix2[i] = i;

        double[,] res = new double[matrix2.Length, coef.Length];
        for (int i = 0; i < matrix2.Length; i++)
            for (int j = 0; j < coef.Length; j++)
                res[i, j] = Math.Pow(coef[j], matrix2[i]);

        double[] y = new double[_polyOrder + 1];
        y[_derivativeOrder] = _derivativeOrder.Factorial() / Math.Pow(1, _derivativeOrder);

        coef = Matrix.Solve(res, y);

        return coef;
    }

    public double[] ProcessSequence(double[] signalArray) => signalArray.Convolve(_coef, true);

    public List<double> ProcessSequence(List<double> inputModel) {
        int initLength = inputModel.Count;
        int halfWindow = (_windowSize + 1) / 2;

        var x = Enumerable.Range(0, _windowSize).Select(m => (double)m).ToArray();
        var xLeft = Enumerable.Range(-halfWindow, halfWindow).Select(m => (double)m).ToArray();
        var xRight = Enumerable.Range(_windowSize, halfWindow).Select(m => (double)m).ToArray();

        var yLeft = inputModel.Take(_windowSize).ToArray();
        var yRight = inputModel.TakeLast(_windowSize).ToArray();

        var ols = new OrdinaryLeastSquares();
        var regLeft = ols.Learn(x, yLeft);
        var regRight = ols.Learn(x, yRight);

        var yExpectedLeft = regLeft.Transform(xLeft);
        var yExpectedRight = regRight.Transform(xRight);

        var newSequence = new List<double>(yExpectedRight.Length + yExpectedLeft.Length + initLength);
        newSequence.AddRange(yExpectedLeft);
        newSequence.AddRange(inputModel);
        newSequence.AddRange(yExpectedRight);

        return newSequence.ToArray().Convolve(_coef, true).Skip(halfWindow).Take(initLength).ToList();
    }

}