namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adjustment bundle policy.</summary>
public static class AdjustmentBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 53453.0, in order.</summary>
    public static int AdjustmentBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53453.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentBundle()
    {
        var policy = Retry.DefaultPolicy("adjustment-bundle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a adjustment bundle score falls in.</summary>
    public static string ClassifyAdjustmentBundle(double score)
    {
        if (score < 3.1)
        {
            return "minimal";
        }
        if (score < 12.3)
        {
            return "ok";
        }
        if (score < 44.4)
        {
            return "critical";
        }
        if (score < 88.9)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a adjustment bundle code in its canonical AB- form.</summary>
    public static string NormalizeAdjustmentBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
