namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of statement of work record policy.</summary>
public static class StatementOfWorkRecordPolicy
{
    public static int StatementOfWorkRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a statement of work record score falls in.</summary>
    public static string ClassifyStatementOfWorkRecord(double score)
    {
        if (score < 4.6)
        {
            return "watch";
        }
        if (score < 6.2)
        {
            return "ok";
        }
        if (score < 27.7)
        {
            return "minimal";
        }
        if (score < 37.8)
        {
            return "elevated";
        }
        return "low";
    }

    public static List<string> MergeStatementOfWorkRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkRecord()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-record");
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
