namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Functions of batched mobility manifest from audit.</summary>
public static class BatchedMobilityManifestFromAudit
{
    public static Dictionary<string, double> SummarizeBatchedSkillProfileAuditsForManifest(List<BatchedSkillProfileAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<BatchedMobilityManifest> DeriveBatchedMobilityManifestsFromAudit(List<BatchedSkillProfileAudit> items)
    {
        var outValue = new List<BatchedMobilityManifest>();
        foreach (var item in items)
        {
            var derived = BatchedMobilityManifest.NewBatchedMobilityManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBatchedMobilityManifest(BatchedMobilityManifest item)
    {
        return SkillProfileAdjustmentPolicy.SkillProfileAdjustmentBucket(item.NetTotal());
    }
}
