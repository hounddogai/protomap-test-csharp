namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Functions of reorg draft from rollup.</summary>
public static class ReorgDraftFromRollup
{
    public static Dictionary<string, double> SummarizeReorgRollupsForDraft(List<ReorgRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<ReorgDraft> DeriveReorgDraftsFromRollup(List<ReorgRollup> items)
    {
        var outValue = new List<ReorgDraft>();
        foreach (var item in items)
        {
            var derived = ReorgDraft.NewReorgDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleReorgDraft(ReorgDraft item)
    {
        return ReorgClaimPolicy.ReorgClaimWeight(9, true);
    }
}
