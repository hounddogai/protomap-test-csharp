namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Functions of manual bonus pool draft from proposal.</summary>
public static class ManualBonusPoolDraftFromProposal
{
    public static Dictionary<string, double> SummarizeEquityGrantProposalsForDraft(List<EquityGrantProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ManualBonusPoolDraft> DeriveManualBonusPoolDraftsFromProposal(List<EquityGrantProposal> items)
    {
        var outValue = new List<ManualBonusPoolDraft>();
        foreach (var item in items)
        {
            var derived = ManualBonusPoolDraft.NewManualBonusPoolDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignManualBonusPoolDraft(ManualBonusPoolDraft item)
    {
        return ScheduledEquityGrantQuotaPolicy.NormalizeScheduledEquityGrantQuotaCode(item.Code);
    }
}
