namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh entry policy.</summary>
public static class RefreshEntryPolicy
{
    /// <summary>Puts a refresh entry code in its canonical RE- form.</summary>
    public static string NormalizeRefreshEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RefreshEntrySpan(int start, int end)
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
        return span / 60;
    }

    public static List<string> MergeRefreshEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
