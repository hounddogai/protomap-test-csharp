namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback license seat reservation.</summary>
public sealed record FallbackLicenseSeatReservationView(string CodeText, string Name, string Title, string MarginText, string Weight, string Retries, string TimezoneText, string JobCode, string StatusLabel)
{
    public static FallbackLicenseSeatReservationView ToFallbackLicenseSeatReservationView(FallbackLicenseSeatReservation item)
    {
        return new FallbackLicenseSeatReservationView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLicenseSeatReservationView> ToFallbackLicenseSeatReservationViews(List<FallbackLicenseSeatReservation> items)
    {
        var views = new List<FallbackLicenseSeatReservationView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLicenseSeatReservationView(item));
        }
        return views;
    }

    public static double SumFallbackLicenseSeatReservationView(FallbackLicenseSeatReservationView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedMargin + parsedWeight + (double)parsedRetries;
    }
}
