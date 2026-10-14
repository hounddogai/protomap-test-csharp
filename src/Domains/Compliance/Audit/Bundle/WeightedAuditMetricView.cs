namespace Acme.Workforce.Compliance.Audit.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted audit metric.</summary>
public sealed record WeightedAuditMetricView(string Code, string Title, string ExpiresAtText, string Approved, string WindowDays, string Budget, string Segment, string StatusLabel)
{
    public static WeightedAuditMetricView ToWeightedAuditMetricView(WeightedAuditMetric item)
    {
        return new WeightedAuditMetricView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAuditMetricView> ToWeightedAuditMetricViews(List<WeightedAuditMetric> items)
    {
        var views = new List<WeightedAuditMetricView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAuditMetricView(item));
        }
        return views;
    }

    public static double SumWeightedAuditMetricView(WeightedAuditMetricView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedWindowDays + parsedBudget;
    }
}
