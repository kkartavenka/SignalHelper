namespace SignalHelper.Classes.Extensions;

internal static class NumericExtension {
    internal static uint Factorial(this int order) {
        uint returnVar = 1;
        for (uint i = 1; i < order; i++)
            returnVar *= i;

        return returnVar;
    }
}
