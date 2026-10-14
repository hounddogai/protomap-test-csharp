namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched certification audit policy.</summary>
public static class BatchedCertificationAuditPolicy
{
    /// <summary>Puts a batched certification audit code in its canonical BCA- form.</summary>
    public static string NormalizeBatchedCertificationAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a batched certification audit score falls in.</summary>
    public static string ClassifyBatchedCertificationAudit(double score)
    {
        if (score < 5.6)
        {
            return "watch";
        }
        if (score < 20.4)
        {
            return "minimal";
        }
        if (score < 43.8)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedCertificationAudit()
    {
        var policy = Retry.DefaultPolicy("batched-certification-audit");
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
}
