namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using System.Globalization;

/// <summary>How screens and exports show a adaptive accrual metric.</summary>
public sealed record AdaptiveAccrualMetricView(string Code, string Title, string ExternalRef, string Priority, string Retries, string Threshold, string Description, string StartsAtText, string Summary, string ContactEmail, string StatusLabel)
{
    public static AdaptiveAccrualMetricView ToAdaptiveAccrualMetricView(AdaptiveAccrualMetric item)
    {
        return new AdaptiveAccrualMetricView(
            item.Code,
            item.Title,
            item.ExternalRef,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAccrualMetricView> ToAdaptiveAccrualMetricViews(List<AdaptiveAccrualMetric> items)
    {
        var views = new List<AdaptiveAccrualMetricView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAccrualMetricView(item));
        }
        return views;
    }

    public static double SumAdaptiveAccrualMetricView(AdaptiveAccrualMetricView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedRetries + parsedThreshold;
    }
}
