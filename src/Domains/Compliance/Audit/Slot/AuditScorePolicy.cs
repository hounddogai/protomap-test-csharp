namespace Acme.Workforce.Compliance.Audit.Slot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit score policy.</summary>
public static class AuditScorePolicy
{
    /// <summary>Names the band a audit score score falls in.</summary>
    public static string ClassifyAuditScore(double score)
    {
        if (score < 4.0)
        {
            return "low";
        }
        if (score < 33.0)
        {
            return "medium";
        }
        if (score < 52.6)
        {
            return "high";
        }
        if (score < 90.3)
        {
            return "elevated";
        }
        return "watch";
    }

    public static double AuditScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a audit score code in its canonical AS- form.</summary>
    public static string NormalizeAuditScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditScore()
    {
        var policy = Retry.DefaultPolicy("audit-score");
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
