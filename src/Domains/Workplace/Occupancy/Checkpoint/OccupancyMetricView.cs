namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy metric.</summary>
public sealed record OccupancyMetricView(string Code, string Name, string Rating, string Notes, string ReviewerText, string Discount, string ExternalRef, string Minimum, string StatusLabel)
{
    public static OccupancyMetricView ToOccupancyMetricView(OccupancyMetric item)
    {
        return new OccupancyMetricView(
            item.Code,
            item.Name,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            Text.Truncate(item.Reviewer, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyMetricView> ToOccupancyMetricViews(List<OccupancyMetric> items)
    {
        var views = new List<OccupancyMetricView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyMetricView(item));
        }
        return views;
    }

    public static double SumOccupancyMetricView(OccupancyMetricView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedRating + parsedDiscount + parsedMinimum;
    }
}
