namespace Acme.Workforce.Workplace.Booking.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a booking budget.</summary>
public sealed record BookingBudgetView(string Code, string Label, string BatchSize, string Active, string Description, string Name, string StatusLabel)
{
    public static BookingBudgetView ToBookingBudgetView(BookingBudget item)
    {
        return new BookingBudgetView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Description,
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BookingBudgetView> ToBookingBudgetViews(List<BookingBudget> items)
    {
        var views = new List<BookingBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToBookingBudgetView(item));
        }
        return views;
    }

    public static double SumBookingBudgetView(BookingBudgetView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
