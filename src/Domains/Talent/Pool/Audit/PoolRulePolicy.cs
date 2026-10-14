namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool rule policy.</summary>
public static class PoolRulePolicy
{
    public static double PoolRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PoolRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a pool rule code in its canonical PR- form.</summary>
    public static string NormalizePoolRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
