namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of leave request score policy.</summary>
public static class LeaveRequestScorePolicy
{
    public static List<string> MergeLeaveRequestScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a leave request score score falls in.</summary>
    public static string ClassifyLeaveRequestScore(double score)
    {
        if (score < 22.8)
        {
            return "low";
        }
        if (score < 26.4)
        {
            return "watch";
        }
        if (score < 79.5)
        {
            return "ok";
        }
        if (score < 79.8)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestScore()
    {
        var policy = Retry.DefaultPolicy("leave-request-score");
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

    /// <summary>Puts a leave request score code in its canonical LRS- form.</summary>
    public static string NormalizeLeaveRequestScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
