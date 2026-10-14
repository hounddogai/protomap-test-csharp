namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Functions of engagement batch from template.</summary>
public static class EngagementBatchFromTemplate
{
    public static Dictionary<string, double> SummarizeEngagementTemplatesForBatch(List<EngagementTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<EngagementBatch> DeriveEngagementBatchesFromTemplate(List<EngagementTemplate> items)
    {
        var outValue = new List<EngagementBatch>();
        foreach (var item in items)
        {
            var derived = EngagementBatch.NewEngagementBatch(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateEngagementBatch(EngagementBatch item)
    {
        return LegacyEngagementBudgetPolicy.NormalizeLegacyEngagementBudgetCode(item.Code);
    }
}
