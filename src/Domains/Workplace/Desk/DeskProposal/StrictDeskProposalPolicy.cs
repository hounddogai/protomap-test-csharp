namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict desk proposal policy.</summary>
public static class StrictDeskProposalPolicy
{
    public static List<string> MergeStrictDeskProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a strict desk proposal code in its canonical SDP- form.</summary>
    public static string NormalizeStrictDeskProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
