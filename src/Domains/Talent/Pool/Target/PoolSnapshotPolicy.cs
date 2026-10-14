namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool snapshot policy.</summary>
public static class PoolSnapshotPolicy
{
    /// <summary>Puts a pool snapshot code in its canonical PS- form.</summary>
    public static string NormalizePoolSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePoolSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolSnapshot()
    {
        var policy = Retry.DefaultPolicy("pool-snapshot");
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
}
