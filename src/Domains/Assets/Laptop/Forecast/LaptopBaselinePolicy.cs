namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of laptop baseline policy.</summary>
public static class LaptopBaselinePolicy
{
    public static string LaptopBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double LaptopBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLaptopBaseline()
    {
        var policy = Retry.DefaultPolicy("laptop-baseline");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
