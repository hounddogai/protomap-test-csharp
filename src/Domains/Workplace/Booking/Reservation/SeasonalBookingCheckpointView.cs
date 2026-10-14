namespace Acme.Workforce.Workplace.Booking.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a seasonal booking checkpoint.</summary>
public sealed record SeasonalBookingCheckpointView(string CodeText, string Label, string EndsAt, string GraceDays, string Margin, string LeaveDaysText, string StatusLabel)
{
    public static SeasonalBookingCheckpointView ToSeasonalBookingCheckpointView(SeasonalBookingCheckpoint item)
    {
        return new SeasonalBookingCheckpointView(
            item.Code,
            item.Label,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalBookingCheckpointView> ToSeasonalBookingCheckpointViews(List<SeasonalBookingCheckpoint> items)
    {
        var views = new List<SeasonalBookingCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalBookingCheckpointView(item));
        }
        return views;
    }

    public static double SumSeasonalBookingCheckpointView(SeasonalBookingCheckpointView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedGraceDays + parsedMargin;
    }
}
