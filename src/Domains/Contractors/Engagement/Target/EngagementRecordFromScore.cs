namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

/// <summary>Functions of engagement record from score.</summary>
public static class EngagementRecordFromScore
{
    public static Dictionary<string, double> SummarizeGlobalOffboardingScoresForRecord(List<GlobalOffboardingScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<EngagementRecord> DeriveEngagementRecordsFromScore(List<GlobalOffboardingScore> items)
    {
        var outValue = new List<EngagementRecord>();
        foreach (var item in items)
        {
            var derived = EngagementRecord.NewEngagementRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeEngagementRecord(EngagementRecord item)
    {
        return GlobalOffboardingScorePolicy.GlobalOffboardingScoreBucket(item.EffectiveTotal());
    }
}
