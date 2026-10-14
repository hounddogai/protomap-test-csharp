namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination metric policy.</summary>
public static class NominationMetricPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationMetric()
    {
        var policy = Retry.DefaultPolicy("nomination-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a nomination metric code in its canonical NM- form.</summary>
    public static string NormalizeNominationMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
