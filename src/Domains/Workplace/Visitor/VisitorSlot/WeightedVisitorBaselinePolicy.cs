namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted visitor baseline policy.</summary>
public static class WeightedVisitorBaselinePolicy
{
    /// <summary>Puts a weighted visitor baseline code in its canonical WVB- form.</summary>
    public static string NormalizeWeightedVisitorBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WVB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WVB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWeightedVisitorBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string WeightedVisitorBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
