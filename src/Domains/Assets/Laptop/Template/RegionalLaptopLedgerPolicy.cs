namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional laptop ledger policy.</summary>
public static class RegionalLaptopLedgerPolicy
{
    /// <summary>Names the band a regional laptop ledger score falls in.</summary>
    public static string ClassifyRegionalLaptopLedger(double score)
    {
        if (score < 42.7)
        {
            return "elevated";
        }
        if (score < 85.9)
        {
            return "medium";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalLaptopLedger()
    {
        var policy = Retry.DefaultPolicy("regional-laptop-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string RegionalLaptopLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double RegionalLaptopLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
