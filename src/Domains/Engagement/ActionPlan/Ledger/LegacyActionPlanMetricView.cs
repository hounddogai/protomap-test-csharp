namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using System.Globalization;

/// <summary>How screens and exports show a legacy action plan metric.</summary>
public sealed record LegacyActionPlanMetricView(string CodeText, string Label, string Quantity, string PriorityText, string Minimum, string ManagerId, string ContactEmailText, string StatusLabel)
{
    public static LegacyActionPlanMetricView ToLegacyActionPlanMetricView(LegacyActionPlanMetric item)
    {
        return new LegacyActionPlanMetricView(
            item.Code,
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyActionPlanMetricView> ToLegacyActionPlanMetricViews(List<LegacyActionPlanMetric> items)
    {
        var views = new List<LegacyActionPlanMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyActionPlanMetricView(item));
        }
        return views;
    }

    public static double SumLegacyActionPlanMetricView(LegacyActionPlanMetricView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedPriority + parsedMinimum;
    }
}
