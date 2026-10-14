namespace Acme.Workforce.Compensation.PayBand.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default pay band reservation.</summary>
public sealed record DefaultPayBandReservationView(string Code, string TitleText, string CreatedAt, string Fte, string VersionText, string Retries, string WindowDays, string StatusLabel)
{
    public static DefaultPayBandReservationView ToDefaultPayBandReservationView(DefaultPayBandReservation item)
    {
        return new DefaultPayBandReservationView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPayBandReservationView> ToDefaultPayBandReservationViews(List<DefaultPayBandReservation> items)
    {
        var views = new List<DefaultPayBandReservationView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPayBandReservationView(item));
        }
        return views;
    }

    public static double SumDefaultPayBandReservationView(DefaultPayBandReservationView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedFte + (double)parsedVersion;
    }
}
