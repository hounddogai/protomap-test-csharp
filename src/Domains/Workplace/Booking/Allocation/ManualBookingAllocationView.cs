namespace Acme.Workforce.Workplace.Booking.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual booking allocation.</summary>
public sealed record ManualBookingAllocationView(string Code, string Label, string Capacity, string Budget, string Currency, string Category, string StatusLabel)
{
    public static ManualBookingAllocationView ToManualBookingAllocationView(ManualBookingAllocation item)
    {
        return new ManualBookingAllocationView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualBookingAllocationView> ToManualBookingAllocationViews(List<ManualBookingAllocation> items)
    {
        var views = new List<ManualBookingAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToManualBookingAllocationView(item));
        }
        return views;
    }

    public static double SumManualBookingAllocationView(ManualBookingAllocationView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedBudget;
    }
}
