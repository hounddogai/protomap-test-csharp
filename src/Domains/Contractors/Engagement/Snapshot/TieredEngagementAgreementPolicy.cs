namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered engagement agreement policy.</summary>
public static class TieredEngagementAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredEngagementAgreement()
    {
        var policy = Retry.DefaultPolicy("tiered-engagement-agreement");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a tiered engagement agreement code in its canonical TEA- form.</summary>
    public static string NormalizeTieredEngagementAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TEA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TEA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
