namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback access badge override policy.</summary>
public static class FallbackAccessBadgeOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackAccessBadgeOverride()
    {
        var policy = Retry.DefaultPolicy("fallback-access-badge-override");
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

    public static List<string> MergeFallbackAccessBadgeOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a fallback access badge override code in its canonical FABO- form.</summary>
    public static string NormalizeFallbackAccessBadgeOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FABO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FABO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
