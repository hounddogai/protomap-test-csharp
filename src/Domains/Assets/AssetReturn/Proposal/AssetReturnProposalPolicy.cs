namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return proposal policy.</summary>
public static class AssetReturnProposalPolicy
{
    /// <summary>Names the band a asset return proposal score falls in.</summary>
    public static string ClassifyAssetReturnProposal(double score)
    {
        if (score < 13.6)
        {
            return "low";
        }
        if (score < 94.6)
        {
            return "medium";
        }
        if (score < 95.7)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Puts a asset return proposal code in its canonical ARP- form.</summary>
    public static string NormalizeAssetReturnProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAssetReturnProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
