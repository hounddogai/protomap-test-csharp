namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of position proposal policy.</summary>
public static class PositionProposalPolicy
{
    public static List<string> MergePositionProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a position proposal code in its canonical PP- form.</summary>
    public static string NormalizePositionProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
