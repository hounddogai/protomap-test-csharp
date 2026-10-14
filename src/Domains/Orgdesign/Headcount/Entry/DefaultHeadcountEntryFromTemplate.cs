namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Functions of default headcount entry from template.</summary>
public static class DefaultHeadcountEntryFromTemplate
{
    public static Dictionary<string, double> SummarizeBatchedHeadcountTemplatesForEntry(List<BatchedHeadcountTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<DefaultHeadcountEntry> DeriveDefaultHeadcountEntriesFromTemplate(List<BatchedHeadcountTemplate> items)
    {
        var outValue = new List<DefaultHeadcountEntry>();
        foreach (var item in items)
        {
            var derived = DefaultHeadcountEntry.NewDefaultHeadcountEntry(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectDefaultHeadcountEntry(DefaultHeadcountEntry item)
    {
        return BatchedHeadcountTemplatePolicy.ClassifyBatchedHeadcountTemplate(item.ProjectedTotal());
    }
}
