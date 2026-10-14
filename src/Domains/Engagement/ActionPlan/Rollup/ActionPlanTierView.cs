namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan tier.</summary>
public sealed record ActionPlanTierView(string CodeText, string Label, string RetriesText, string TenureMonths, string JobCode, string FloorValue, string StatusLabel)
{
    public static ActionPlanTierView ToActionPlanTierView(ActionPlanTier item)
    {
        return new ActionPlanTierView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanTierView> ToActionPlanTierViews(List<ActionPlanTier> items)
    {
        var views = new List<ActionPlanTierView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanTierView(item));
        }
        return views;
    }

    public static double SumActionPlanTierView(ActionPlanTierView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedTenureMonths + parsedFloorValue;
    }
}
