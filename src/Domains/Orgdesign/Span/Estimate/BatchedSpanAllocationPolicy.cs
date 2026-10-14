namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched span allocation policy.</summary>
public static class BatchedSpanAllocationPolicy
{
    /// <summary>Puts a batched span allocation code in its canonical BSA- form.</summary>
    public static string NormalizeBatchedSpanAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBatchedSpanAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
