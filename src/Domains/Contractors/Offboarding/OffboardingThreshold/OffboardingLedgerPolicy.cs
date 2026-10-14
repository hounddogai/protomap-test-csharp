namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of offboarding ledger policy.</summary>
public static class OffboardingLedgerPolicy
{
    /// <summary>Puts a offboarding ledger code in its canonical OL- form.</summary>
    public static string NormalizeOffboardingLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOffboardingLedger()
    {
        var policy = Retry.DefaultPolicy("offboarding-ledger");
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

    /// <summary>Names the band a offboarding ledger score falls in.</summary>
    public static string ClassifyOffboardingLedger(double score)
    {
        if (score < 23.0)
        {
            return "elevated";
        }
        if (score < 84.3)
        {
            return "low";
        }
        return "watch";
    }
}
