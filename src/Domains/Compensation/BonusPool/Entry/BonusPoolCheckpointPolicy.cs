namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of bonus pool checkpoint policy.</summary>
public static class BonusPoolCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBonusPoolCheckpoint()
    {
        var policy = Retry.DefaultPolicy("bonus-pool-checkpoint");
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

    public static string BonusPoolCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double BonusPoolCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a bonus pool checkpoint code in its canonical BPC- form.</summary>
    public static string NormalizeBonusPoolCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
