namespace Acme.Workforce.Absence.Holiday.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal holiday claim.</summary>
public sealed record SeasonalHolidayClaimView(string Code, string Name, string Segment, string Rating, string Priority, string ManagerId, string SalaryBand, string ExpiresAt, string Hours, string StatusLabel)
{
    public static SeasonalHolidayClaimView ToSeasonalHolidayClaimView(SeasonalHolidayClaim item)
    {
        return new SeasonalHolidayClaimView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Segment,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.SalaryBand,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalHolidayClaimView> ToSeasonalHolidayClaimViews(List<SeasonalHolidayClaim> items)
    {
        var views = new List<SeasonalHolidayClaimView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalHolidayClaimView(item));
        }
        return views;
    }

    public static double SumSeasonalHolidayClaimView(SeasonalHolidayClaimView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedPriority + (double)parsedExpiresAt;
    }
}
