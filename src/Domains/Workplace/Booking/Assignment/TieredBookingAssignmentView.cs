namespace Acme.Workforce.Workplace.Booking.Assignment;

using System.Globalization;

/// <summary>How screens and exports show a tiered booking assignment.</summary>
public sealed record TieredBookingAssignmentView(string Code, string LabelText, string OwnerIdText, string EmployeeIdText, string Quantity, string ContactEmailText, string Threshold, string BadgeText, string EndsAt, string StatusLabel)
{
    public static TieredBookingAssignmentView ToTieredBookingAssignmentView(TieredBookingAssignment item)
    {
        return new TieredBookingAssignmentView(
            item.Code,
            item.Label,
            item.OwnerId,
            item.EmployeeId,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredBookingAssignmentView> ToTieredBookingAssignmentViews(List<TieredBookingAssignment> items)
    {
        var views = new List<TieredBookingAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToTieredBookingAssignmentView(item));
        }
        return views;
    }

    public static double SumTieredBookingAssignmentView(TieredBookingAssignmentView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedThreshold + (double)parsedEndsAt;
    }
}
