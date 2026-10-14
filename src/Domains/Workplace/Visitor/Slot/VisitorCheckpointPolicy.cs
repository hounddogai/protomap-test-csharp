namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor checkpoint policy.</summary>
public static class VisitorCheckpointPolicy
{
    public static int VisitorCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorCheckpoint()
    {
        var policy = Retry.DefaultPolicy("visitor-checkpoint");
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

    public static List<string> MergeVisitorCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a visitor checkpoint code in its canonical VC- form.</summary>
    public static string NormalizeVisitorCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
