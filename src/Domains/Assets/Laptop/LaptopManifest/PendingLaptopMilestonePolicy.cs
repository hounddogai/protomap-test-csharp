namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending laptop milestone policy.</summary>
public static class PendingLaptopMilestonePolicy
{
    public static int PendingLaptopMilestoneSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingLaptopMilestone()
    {
        var policy = Retry.DefaultPolicy("pending-laptop-milestone");
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

    public static List<string> MergePendingLaptopMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a pending laptop milestone code in its canonical PLM- form.</summary>
    public static string NormalizePendingLaptopMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PLM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PLM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
