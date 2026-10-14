namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a default adjustment estimate.</summary>
public sealed record DefaultAdjustmentEstimateView(string CodeText, string Name, string RateText, string DurationMinutes, string Region, string RatingText, string MinimumText, string ApprovedText, string TitleText, string ChecksumText, string StatusLabel)
{
    public static DefaultAdjustmentEstimateView ToDefaultAdjustmentEstimateView(DefaultAdjustmentEstimate item)
    {
        return new DefaultAdjustmentEstimateView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Title,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAdjustmentEstimateView> ToDefaultAdjustmentEstimateViews(List<DefaultAdjustmentEstimate> items)
    {
        var views = new List<DefaultAdjustmentEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAdjustmentEstimateView(item));
        }
        return views;
    }

    public static double SumDefaultAdjustmentEstimateView(DefaultAdjustmentEstimateView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedDurationMinutes + parsedRating;
    }
}
