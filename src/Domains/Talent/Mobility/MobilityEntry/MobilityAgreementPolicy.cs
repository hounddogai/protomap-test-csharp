namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of mobility agreement policy.</summary>
public static class MobilityAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMobilityAgreement()
    {
        var policy = Retry.DefaultPolicy("mobility-agreement");
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

    /// <summary>Puts a mobility agreement code in its canonical MA- form.</summary>
    public static string NormalizeMobilityAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double MobilityAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
