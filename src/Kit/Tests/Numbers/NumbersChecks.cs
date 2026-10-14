namespace Acme.Workforce.Kit.Tests.Numbers;

using Acme.Workforce.Kit.Numbers;

/// <summary>Checks of numbers code, run by the project's runner.</summary>
public static class NumbersChecks
{
    public static void ClampsIntoRange()
    {
        Check.Equal(Numbers.Clamp(12, 0, 10), 10, "high");
        Check.Equal(Numbers.Clamp(-3, 0, 10), 0, "low");
        Check.Equal(Numbers.Percent(1.0, 4.0), 25.0, "percent");
    }

    public static void FindsTheMedianByRank()
    {
        Check.Equal(Numbers.Percentile(new List<double> { 5.0, 1.0, 3.0 }, 50.0), 3.0, "median");
        Check.Equal(Numbers.Mean(new List<double>()), 0.0, "empty mean");
        Check.Fails(() => _ = Numbers.WeightedMean(new List<double> { 1.0 }, new List<double>()), "mismatched weights");
    }

    public static void RunAll()
    {
        ClampsIntoRange();
        FindsTheMedianByRank();
    }
}
