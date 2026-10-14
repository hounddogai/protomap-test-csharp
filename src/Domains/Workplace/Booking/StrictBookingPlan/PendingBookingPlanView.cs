namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending booking plan.</summary>
public sealed record PendingBookingPlanView(string Code, string Label, string Sequence, string CapacityText, string Limit, string ArchivedText, string StatusLabel)
{
    public static PendingBookingPlanView ToPendingBookingPlanView(PendingBookingPlan item)
    {
        return new PendingBookingPlanView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingBookingPlanView> ToPendingBookingPlanViews(List<PendingBookingPlan> items)
    {
        var views = new List<PendingBookingPlanView>();
        foreach (var item in items)
        {
            views.Add(ToPendingBookingPlanView(item));
        }
        return views;
    }

    public static double SumPendingBookingPlanView(PendingBookingPlanView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedCapacity + (double)parsedLimit;
    }
}
