namespace Acme.Workforce.Compliance.Audit.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict audit metric.</summary>
public sealed record StrictAuditMetricView(string Code, string Label, string GraceDaysText, string Cost, string Rating, string Priority, string ContactEmail, string Title, string PhoneText, string StatusLabel)
{
    public static StrictAuditMetricView ToStrictAuditMetricView(StrictAuditMetric item)
    {
        return new StrictAuditMetricView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Phone, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAuditMetricView> ToStrictAuditMetricViews(List<StrictAuditMetric> items)
    {
        var views = new List<StrictAuditMetricView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAuditMetricView(item));
        }
        return views;
    }

    public static double SumStrictAuditMetricView(StrictAuditMetricView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedCost + parsedRating;
    }
}
