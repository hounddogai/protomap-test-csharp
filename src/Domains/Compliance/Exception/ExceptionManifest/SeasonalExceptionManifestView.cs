namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using System.Globalization;

/// <summary>How screens and exports show a seasonal exception manifest.</summary>
public sealed record SeasonalExceptionManifestView(string Code, string Label, string Description, string Rating, string CapacityText, string WindowDaysText, string StatusLabel)
{
    public static SeasonalExceptionManifestView ToSeasonalExceptionManifestView(SeasonalExceptionManifest item)
    {
        return new SeasonalExceptionManifestView(
            item.Code,
            item.Label,
            item.Description,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalExceptionManifestView> ToSeasonalExceptionManifestViews(List<SeasonalExceptionManifest> items)
    {
        var views = new List<SeasonalExceptionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalExceptionManifestView(item));
        }
        return views;
    }

    public static double SumSeasonalExceptionManifestView(SeasonalExceptionManifestView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCapacity + (double)parsedWindowDays;
    }
}
