namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback pool draft policy.</summary>
public static class FallbackPoolDraftPolicy
{
    public static List<string> MergeFallbackPoolDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int FallbackPoolDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a fallback pool draft code in its canonical FPD- form.</summary>
    public static string NormalizeFallbackPoolDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
