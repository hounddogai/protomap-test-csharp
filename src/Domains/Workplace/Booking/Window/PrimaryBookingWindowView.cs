namespace Acme.Workforce.Workplace.Booking.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary booking window.</summary>
public sealed record PrimaryBookingWindowView(string Code, string NameText, string StartsAt, string Margin, string DepartmentText, string Skill, string Ceiling, string PhoneText, string Cost, string Currency, string StatusLabel)
{
    public static PrimaryBookingWindowView ToPrimaryBookingWindowView(PrimaryBookingWindow item)
    {
        return new PrimaryBookingWindowView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 80),
            Text.Truncate(item.Skill, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryBookingWindowView> ToPrimaryBookingWindowViews(List<PrimaryBookingWindow> items)
    {
        var views = new List<PrimaryBookingWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryBookingWindowView(item));
        }
        return views;
    }

    public static double SumPrimaryBookingWindowView(PrimaryBookingWindowView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMargin + parsedCeiling;
    }
}
