namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default reorg tier policy.</summary>
public static class DefaultReorgTierPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultReorgTier()
    {
        var policy = Retry.DefaultPolicy("default-reorg-tier");
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

    /// <summary>Names the band a default reorg tier score falls in.</summary>
    public static string ClassifyDefaultReorgTier(double score)
    {
        if (score < 43.4)
        {
            return "critical";
        }
        if (score < 71.7)
        {
            return "elevated";
        }
        if (score < 72.5)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Puts a default reorg tier code in its canonical DRT- form.</summary>
    public static string NormalizeDefaultReorgTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 28391.0, in order.</summary>
    public static int DefaultReorgTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28391.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
