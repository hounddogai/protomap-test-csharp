namespace Acme.Workforce.Workplace.Booking.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped booking target.</summary>
public sealed record CappedBookingTargetView(string Code, string Name, string ContactEmail, string AmountText, string CostCenter, string Segment, string Ceiling, string Hours, string StatusLabel)
{
    public static CappedBookingTargetView ToCappedBookingTargetView(CappedBookingTarget item)
    {
        return new CappedBookingTargetView(
            item.Code,
            item.Name,
            Text.Truncate(item.ContactEmail, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Segment,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedBookingTargetView> ToCappedBookingTargetViews(List<CappedBookingTarget> items)
    {
        var views = new List<CappedBookingTargetView>();
        foreach (var item in items)
        {
            views.Add(ToCappedBookingTargetView(item));
        }
        return views;
    }

    public static double SumCappedBookingTargetView(CappedBookingTargetView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedAmount + parsedCeiling + parsedHours;
    }
}
