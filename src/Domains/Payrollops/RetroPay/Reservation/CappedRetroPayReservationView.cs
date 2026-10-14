namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped retro pay reservation.</summary>
public sealed record CappedRetroPayReservationView(string Code, string NameText, string ActiveText, string HoursText, string ExpiresAtText, string NotesText, string Ratio, string EmployeeId, string JobCode, string StatusLabel)
{
    public static CappedRetroPayReservationView ToCappedRetroPayReservationView(CappedRetroPayReservation item)
    {
        return new CappedRetroPayReservationView(
            item.Code,
            item.Name,
            $"{item.Active}",
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 80),
            Text.Truncate(item.JobCode, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRetroPayReservationView> ToCappedRetroPayReservationViews(List<CappedRetroPayReservation> items)
    {
        var views = new List<CappedRetroPayReservationView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRetroPayReservationView(item));
        }
        return views;
    }

    public static double SumCappedRetroPayReservationView(CappedRetroPayReservationView view)
    {
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedExpiresAt + parsedRatio;
    }
}
