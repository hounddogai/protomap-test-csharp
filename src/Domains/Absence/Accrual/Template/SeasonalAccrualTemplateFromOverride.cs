namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Functions of seasonal accrual template from override.</summary>
public static class SeasonalAccrualTemplateFromOverride
{
    public static Dictionary<string, double> SummarizeAccrualOverridesForTemplate(List<AccrualOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalAccrualTemplate> DeriveSeasonalAccrualTemplatesFromOverride(List<AccrualOverride> items)
    {
        var outValue = new List<SeasonalAccrualTemplate>();
        foreach (var item in items)
        {
            var derived = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignSeasonalAccrualTemplate(SeasonalAccrualTemplate item)
    {
        return AccrualOverridePolicy.ClassifyAccrualOverride(item.ProjectedLoad());
    }
}
