namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of access badge snapshot policy.</summary>
public static class AccessBadgeSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeSnapshot()
    {
        var policy = Retry.DefaultPolicy("access-badge-snapshot");
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

    public static List<string> MergeAccessBadgeSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a access badge snapshot code in its canonical ABS- form.</summary>
    public static string NormalizeAccessBadgeSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 16908.0, in order.</summary>
    public static int AccessBadgeSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16908.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
