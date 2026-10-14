namespace Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Functions of global adjustment bundle policy.</summary>
public static class GlobalAdjustmentBundlePolicy
{
    /// <summary>Puts a global adjustment bundle code in its canonical GAB- form.</summary>
    public static string NormalizeGlobalAdjustmentBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a global adjustment bundle score falls in.</summary>
    public static string ClassifyGlobalAdjustmentBundle(double score)
    {
        if (score < 48.4)
        {
            return "ok";
        }
        if (score < 64.4)
        {
            return "critical";
        }
        if (score < 87.2)
        {
            return "low";
        }
        return "medium";
    }

    public static int GlobalAdjustmentBundleSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 46756.0, in order.</summary>
    public static int GlobalAdjustmentBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46756.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
