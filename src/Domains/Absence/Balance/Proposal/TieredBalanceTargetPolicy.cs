namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered balance target policy.</summary>
public static class TieredBalanceTargetPolicy
{
    /// <summary>Puts a tiered balance target code in its canonical TBT- form.</summary>
    public static string NormalizeTieredBalanceTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TieredBalanceTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double TieredBalanceTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredBalanceTarget()
    {
        var policy = Retry.DefaultPolicy("tiered-balance-target");
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
}
