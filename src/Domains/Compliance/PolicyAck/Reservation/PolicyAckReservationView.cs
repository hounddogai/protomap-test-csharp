namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack reservation.</summary>
public sealed record PolicyAckReservationView(string CodeText, string TitleText, string ManagerIdText, string TimezoneText, string Hours, string Cost, string Rating, string ExpiresAt, string Reviewer, string EndsAt, string StatusLabel)
{
    public static PolicyAckReservationView ToPolicyAckReservationView(PolicyAckReservation item)
    {
        return new PolicyAckReservationView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.ManagerId,
            item.Timezone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckReservationView> ToPolicyAckReservationViews(List<PolicyAckReservation> items)
    {
        var views = new List<PolicyAckReservationView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckReservationView(item));
        }
        return views;
    }

    public static double SumPolicyAckReservationView(PolicyAckReservationView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedHours + parsedCost + parsedRating;
    }
}
