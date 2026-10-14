namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled mobility entry policy.</summary>
public static class ScheduledMobilityEntryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledMobilityEntry()
    {
        var policy = Retry.DefaultPolicy("scheduled-mobility-entry");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a scheduled mobility entry code in its canonical SME- form.</summary>
    public static string NormalizeScheduledMobilityEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SME-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SME-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScheduledMobilityEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeScheduledMobilityEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
