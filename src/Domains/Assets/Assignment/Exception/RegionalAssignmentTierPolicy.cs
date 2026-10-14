namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional assignment tier policy.</summary>
public static class RegionalAssignmentTierPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalAssignmentTier()
    {
        var policy = Retry.DefaultPolicy("regional-assignment-tier");
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

    public static List<string> MergeRegionalAssignmentTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a regional assignment tier code in its canonical RAT- form.</summary>
    public static string NormalizeRegionalAssignmentTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
