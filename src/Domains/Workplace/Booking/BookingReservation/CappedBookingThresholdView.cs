namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped booking threshold.</summary>
public sealed record CappedBookingThresholdView(string CodeText, string Name, string CostText, string LocationText, string Skill, string Sequence, string Title, string Attempts, string Priority, string StatusLabel)
{
    public static CappedBookingThresholdView ToCappedBookingThresholdView(CappedBookingThreshold item)
    {
        return new CappedBookingThresholdView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Skill, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBookingThresholdView> ToCappedBookingThresholdViews(List<CappedBookingThreshold> items)
    {
        var views = new List<CappedBookingThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBookingThresholdView(item));
        }
        return views;
    }

    public static double SumCappedBookingThresholdView(CappedBookingThresholdView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedSequence + (double)parsedAttempts;
    }
}
