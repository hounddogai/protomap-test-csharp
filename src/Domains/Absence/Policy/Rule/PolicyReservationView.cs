namespace Acme.Workforce.Absence.Policy.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy reservation.</summary>
public sealed record PolicyReservationView(string CodeText, string Name, string WindowDays, string CountryText, string ManagerIdText, string OwnerIdText, string Margin, string StatusLabel)
{
    public static PolicyReservationView ToPolicyReservationView(PolicyReservation item)
    {
        return new PolicyReservationView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Country,
            Text.Truncate(item.ManagerId, 80),
            item.OwnerId,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyReservationView> ToPolicyReservationViews(List<PolicyReservation> items)
    {
        var views = new List<PolicyReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyReservationView(item));
        }
        return views;
    }

    public static double SumPolicyReservationView(PolicyReservationView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedMargin;
    }
}
