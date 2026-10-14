namespace Acme.Workforce.Contractors.Rate.Plan;

using System.Globalization;

/// <summary>How screens and exports show a rate mapping.</summary>
public sealed record RateMappingView(string Code, string Title, string Visible, string Version, string DurationMinutes, string Phone, string RatingText, string ApprovedText, string StatusLabel)
{
    public static RateMappingView ToRateMappingView(RateMapping item)
    {
        return new RateMappingView(
            item.Code,
            item.Title,
            $"{item.Visible}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateMappingView> ToRateMappingViews(List<RateMapping> items)
    {
        var views = new List<RateMappingView>();
        foreach (var item in items)
        {
            views.Add(ToRateMappingView(item));
        }
        return views;
    }

    public static double SumRateMappingView(RateMappingView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedDurationMinutes + parsedRating;
    }
}
