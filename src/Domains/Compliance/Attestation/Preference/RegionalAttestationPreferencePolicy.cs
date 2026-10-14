namespace Acme.Workforce.Compliance.Attestation.Preference;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional attestation preference policy.</summary>
public static class RegionalAttestationPreferencePolicy
{
    /// <summary>Names the band a regional attestation preference score falls in.</summary>
    public static string ClassifyRegionalAttestationPreference(double score)
    {
        if (score < 18.0)
        {
            return "critical";
        }
        if (score < 19.1)
        {
            return "high";
        }
        if (score < 56.4)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 13453.0, in order.</summary>
    public static int RegionalAttestationPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13453.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a regional attestation preference code in its canonical RAP- form.</summary>
    public static string NormalizeRegionalAttestationPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalAttestationPreference()
    {
        var policy = Retry.DefaultPolicy("regional-attestation-preference");
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
}
