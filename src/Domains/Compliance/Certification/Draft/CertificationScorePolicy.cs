namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of certification score policy.</summary>
public static class CertificationScorePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationScore()
    {
        var policy = Retry.DefaultPolicy("certification-score");
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

    /// <summary>Puts a certification score code in its canonical CS- form.</summary>
    public static string NormalizeCertificationScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
