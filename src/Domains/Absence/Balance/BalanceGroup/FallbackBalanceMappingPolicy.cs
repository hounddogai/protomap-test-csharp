namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback balance mapping policy.</summary>
public static class FallbackBalanceMappingPolicy
{
    /// <summary>Puts a fallback balance mapping code in its canonical FBM- form.</summary>
    public static string NormalizeFallbackBalanceMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FBM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FBM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string FallbackBalanceMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackBalanceMapping()
    {
        var policy = Retry.DefaultPolicy("fallback-balance-mapping");
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

    public static double FallbackBalanceMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
