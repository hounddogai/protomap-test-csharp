namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Functions of primary headcount draft from snapshot.</summary>
public static class PrimaryHeadcountDraftFromSnapshot
{
    public static Dictionary<string, double> SummarizeHeadcountSnapshotsForDraft(List<HeadcountSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<PrimaryHeadcountDraft> DerivePrimaryHeadcountDraftsFromSnapshot(List<HeadcountSnapshot> items)
    {
        var outValue = new List<PrimaryHeadcountDraft>();
        foreach (var item in items)
        {
            var derived = PrimaryHeadcountDraft.NewPrimaryHeadcountDraft(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettlePrimaryHeadcountDraft(PrimaryHeadcountDraft item)
    {
        return LegacyHeadcountBudgetPolicy.LegacyHeadcountBudgetBucket(item.MeasuredValue());
    }
}
