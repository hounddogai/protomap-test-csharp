namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted laptop audit policy.</summary>
public static class WeightedLaptopAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedLaptopAudit()
    {
        var policy = Retry.DefaultPolicy("weighted-laptop-audit");
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

    public static double WeightedLaptopAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a weighted laptop audit code in its canonical WLA- form.</summary>
    public static string NormalizeWeightedLaptopAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WLA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WLA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted laptop audit score falls in.</summary>
    public static string ClassifyWeightedLaptopAudit(double score)
    {
        if (score < 39.9)
        {
            return "low";
        }
        if (score < 71.8)
        {
            return "elevated";
        }
        return "watch";
    }
}
