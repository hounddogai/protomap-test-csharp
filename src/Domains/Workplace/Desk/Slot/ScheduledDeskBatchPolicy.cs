namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled desk batch policy.</summary>
public static class ScheduledDeskBatchPolicy
{
    public static List<string> MergeScheduledDeskBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledDeskBatch()
    {
        var policy = Retry.DefaultPolicy("scheduled-desk-batch");
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

    /// <summary>Puts a scheduled desk batch code in its canonical SDB- form.</summary>
    public static string NormalizeScheduledDeskBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
