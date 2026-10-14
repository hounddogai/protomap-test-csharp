namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk audit policy.</summary>
public static class DeskAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskAudit()
    {
        var policy = Retry.DefaultPolicy("desk-audit");
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

    /// <summary>Names the band a desk audit score falls in.</summary>
    public static string ClassifyDeskAudit(double score)
    {
        if (score < 66.4)
        {
            return "medium";
        }
        if (score < 68.1)
        {
            return "high";
        }
        return "low";
    }

    public static string DeskAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a desk audit code in its canonical DA- form.</summary>
    public static string NormalizeDeskAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
