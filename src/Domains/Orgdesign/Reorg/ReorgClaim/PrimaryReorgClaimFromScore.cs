namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Functions of primary reorg claim from score.</summary>
public static class PrimaryReorgClaimFromScore
{
    public static Dictionary<string, double> SummarizeHeadcountScoresForClaim(List<HeadcountScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PrimaryReorgClaim> DerivePrimaryReorgClaimsFromScore(List<HeadcountScore> items)
    {
        var outValue = new List<PrimaryReorgClaim>();
        foreach (var item in items)
        {
            var derived = PrimaryReorgClaim.NewPrimaryReorgClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePrimaryReorgClaim(PrimaryReorgClaim item)
    {
        return HeadcountTemplatePolicy.NormalizeHeadcountTemplateCode(item.Code);
    }
}
