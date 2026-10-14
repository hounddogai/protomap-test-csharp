namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span manifest policy.</summary>
public static class SpanManifestPolicy
{
    public static List<string> MergeSpanManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a span manifest code in its canonical SM- form.</summary>
    public static string NormalizeSpanManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
