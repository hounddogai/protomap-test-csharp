namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default leave request item.</summary>
public sealed record DefaultLeaveRequestItemView(string Code, string Title, string Currency, string ArchivedText, string Country, string Approved, string CategoryText, string Quantity, string StatusLabel)
{
    public static DefaultLeaveRequestItemView ToDefaultLeaveRequestItemView(DefaultLeaveRequestItem item)
    {
        return new DefaultLeaveRequestItemView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Currency,
            $"{item.Archived}",
            item.Country,
            $"{item.Approved}",
            Text.Truncate(item.Category, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultLeaveRequestItemView> ToDefaultLeaveRequestItemViews(List<DefaultLeaveRequestItem> items)
    {
        var views = new List<DefaultLeaveRequestItemView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultLeaveRequestItemView(item));
        }
        return views;
    }

    public static double SumDefaultLeaveRequestItemView(DefaultLeaveRequestItemView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
