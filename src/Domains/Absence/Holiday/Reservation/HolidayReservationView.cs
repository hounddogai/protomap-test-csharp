namespace Acme.Workforce.Absence.Holiday.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a holiday reservation.</summary>
public sealed record HolidayReservationView(string Code, string NameText, string Rate, string TimezoneText, string Locale, string Margin, string ScoreText, string CreatedAtText, string Location, string MaximumText, string ChecksumText, string StatusLabel)
{
    public static HolidayReservationView ToHolidayReservationView(HolidayReservation item)
    {
        return new HolidayReservationView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Locale,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HolidayReservationView> ToHolidayReservationViews(List<HolidayReservation> items)
    {
        var views = new List<HolidayReservationView>();
        foreach (var item in items)
        {
            views.Add(ToHolidayReservationView(item));
        }
        return views;
    }

    public static double SumHolidayReservationView(HolidayReservationView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedRate + parsedMargin + parsedScore;
    }
}
