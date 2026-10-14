namespace Acme.Workforce.Workplace.Booking.Plan;

using System.Globalization;

/// <summary>How screens and exports show a booking plan.</summary>
public sealed record BookingPlanView(string Code, string NameText, string ArchivedText, string PriorityText, string Weight, string RevisionText, string StatusLabel)
{
    public static BookingPlanView ToBookingPlanView(BookingPlan item)
    {
        return new BookingPlanView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingPlanView> ToBookingPlanViews(List<BookingPlan> items)
    {
        var views = new List<BookingPlanView>();
        foreach (var item in items)
        {
            views.Add(ToBookingPlanView(item));
        }
        return views;
    }

    public static double SumBookingPlanView(BookingPlanView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedWeight + (double)parsedRevision;
    }
}
