namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Functions of merit cycle window from template.</summary>
public static class MeritCycleWindowFromTemplate
{
    public static Dictionary<string, double> SummarizeAdjustmentTemplatesForWindow(List<AdjustmentTemplate> items)
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

    public static List<MeritCycleWindow> DeriveMeritCycleWindowsFromTemplate(List<AdjustmentTemplate> items)
    {
        var outValue = new List<MeritCycleWindow>();
        foreach (var item in items)
        {
            var derived = MeritCycleWindow.NewMeritCycleWindow(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyMeritCycleWindow(MeritCycleWindow item)
    {
        return AdjustmentTemplatePolicy.AdjustmentTemplateBucket(item.MeasuredValue());
    }
}
