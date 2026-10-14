namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk milestone policy.</summary>
public static class DeskMilestonePolicy
{
    /// <summary>Puts a desk milestone code in its canonical DM- form.</summary>
    public static string NormalizeDeskMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskMilestone()
    {
        var policy = Retry.DefaultPolicy("desk-milestone");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeDeskMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
