namespace Acme.Workforce.Contractors.StatementOfWork.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work slot policy.</summary>
public static class StatementOfWorkSlotPolicy
{
    public static List<string> MergeStatementOfWorkSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a statement of work slot score falls in.</summary>
    public static string ClassifyStatementOfWorkSlot(double score)
    {
        if (score < 25.7)
        {
            return "critical";
        }
        if (score < 90.0)
        {
            return "ok";
        }
        if (score < 90.8)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Puts a statement of work slot code in its canonical SOWS- form.</summary>
    public static string NormalizeStatementOfWorkSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkSlot()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-slot");
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
}
