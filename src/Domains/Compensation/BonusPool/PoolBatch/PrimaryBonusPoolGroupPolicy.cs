namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary bonus pool group policy.</summary>
public static class PrimaryBonusPoolGroupPolicy
{
    public static string PrimaryBonusPoolGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryBonusPoolGroup()
    {
        var policy = Retry.DefaultPolicy("primary-bonus-pool-group");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double PrimaryBonusPoolGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a primary bonus pool group code in its canonical PBPG- form.</summary>
    public static string NormalizePrimaryBonusPoolGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBPG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBPG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
