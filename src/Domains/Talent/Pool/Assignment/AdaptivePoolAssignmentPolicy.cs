namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive pool assignment policy.</summary>
public static class AdaptivePoolAssignmentPolicy
{
    public static List<string> MergeAdaptivePoolAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a adaptive pool assignment code in its canonical APA- form.</summary>
    public static string NormalizeAdaptivePoolAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AdaptivePoolAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
