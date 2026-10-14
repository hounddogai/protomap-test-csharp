namespace Acme.Workforce.Workplace.Desk.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk reservation.</summary>
public sealed record DeskReservationView(string CodeText, string LabelText, string CountryText, string Attempts, string StatusLabel)
{
    public static DeskReservationView ToDeskReservationView(DeskReservation item)
    {
        return new DeskReservationView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 64),
            item.Country,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskReservationView> ToDeskReservationViews(List<DeskReservation> items)
    {
        var views = new List<DeskReservationView>();
        foreach (var item in items)
        {
            views.Add(ToDeskReservationView(item));
        }
        return views;
    }

    public static double SumDeskReservationView(DeskReservationView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedAttempts;
    }
}
