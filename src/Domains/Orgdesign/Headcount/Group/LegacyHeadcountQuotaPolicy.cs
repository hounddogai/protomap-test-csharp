namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy headcount quota policy.</summary>
public static class LegacyHeadcountQuotaPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyHeadcountQuota()
    {
        var policy = Retry.DefaultPolicy("legacy-headcount-quota");
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

    /// <summary>Puts a legacy headcount quota code in its canonical LHQ- form.</summary>
    public static string NormalizeLegacyHeadcountQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LHQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LHQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyHeadcountQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
