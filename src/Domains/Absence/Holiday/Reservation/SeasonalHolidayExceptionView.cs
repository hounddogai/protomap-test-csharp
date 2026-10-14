namespace Acme.Workforce.Absence.Holiday.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal holiday exception.</summary>
public sealed record SeasonalHolidayExceptionView(string Code, string NameText, string SummaryText, string LocaleText, string OwnerId, string Discount, string FloorValue, string RetriesText, string Grade, string ExpiresAt, string StatusLabel)
{
    public static SeasonalHolidayExceptionView ToSeasonalHolidayExceptionView(SeasonalHolidayException item)
    {
        return new SeasonalHolidayExceptionView(
            item.Code,
            item.Name,
            Text.Truncate(item.Summary, 40),
            item.Locale,
            item.OwnerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalHolidayExceptionView> ToSeasonalHolidayExceptionViews(List<SeasonalHolidayException> items)
    {
        var views = new List<SeasonalHolidayExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalHolidayExceptionView(item));
        }
        return views;
    }

    public static double SumSeasonalHolidayExceptionView(SeasonalHolidayExceptionView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedFloorValue + (double)parsedRetries;
    }
}
