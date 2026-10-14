namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>Functions of cost model threshold from draft.</summary>
public static class CostModelThresholdFromDraft
{
    public static Dictionary<string, double> SummarizePrimaryHeadcountDraftsForThreshold(List<PrimaryHeadcountDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CostModelThreshold> DeriveCostModelThresholdsFromDraft(List<PrimaryHeadcountDraft> items)
    {
        var outValue = new List<CostModelThreshold>();
        foreach (var item in items)
        {
            var derived = CostModelThreshold.NewCostModelThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeCostModelThreshold(CostModelThreshold item)
    {
        return PrimaryHeadcountDraftPolicy.PrimaryHeadcountDraftBucket(item.ProjectedTotal());
    }
}
