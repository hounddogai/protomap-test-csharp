namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient benchmark digest policy.</summary>
public static class LenientBenchmarkDigestPolicy
{
    public static string LenientBenchmarkDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 42339.0, in order.</summary>
    public static int LenientBenchmarkDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42339.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient benchmark digest score falls in.</summary>
    public static string ClassifyLenientBenchmarkDigest(double score)
    {
        if (score < 16.1)
        {
            return "ok";
        }
        if (score < 77.5)
        {
            return "high";
        }
        if (score < 79.0)
        {
            return "medium";
        }
        return "watch";
    }

    public static double LenientBenchmarkDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
