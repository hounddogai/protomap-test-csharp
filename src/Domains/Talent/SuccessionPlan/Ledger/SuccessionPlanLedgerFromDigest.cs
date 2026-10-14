namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Functions of succession plan ledger from digest.</summary>
public static class SuccessionPlanLedgerFromDigest
{
    public static Dictionary<string, double> SummarizeSkillProfileDigestsForLedger(List<SkillProfileDigest> items)
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

    public static List<SuccessionPlanLedger> DeriveSuccessionPlanLedgersFromDigest(List<SkillProfileDigest> items)
    {
        var outValue = new List<SuccessionPlanLedger>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanLedger.NewSuccessionPlanLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreSuccessionPlanLedger(SuccessionPlanLedger item)
    {
        return SkillProfileDigestPolicy.SkillProfileDigestBucket(item.MeasuredValue());
    }
}
