namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict booking audit.</summary>
public sealed record StrictBookingAuditView(string CodeText, string Label, string FteText, string ManagerIdText, string AmountText, string Team, string Score, string Summary, string EmployeeId, string StatusLabel)
{
    public static StrictBookingAuditView ToStrictBookingAuditView(StrictBookingAudit item)
    {
        return new StrictBookingAuditView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 120),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 80),
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictBookingAuditView> ToStrictBookingAuditViews(List<StrictBookingAudit> items)
    {
        var views = new List<StrictBookingAuditView>();
        foreach (var item in items)
        {
            views.Add(ToStrictBookingAuditView(item));
        }
        return views;
    }

    public static double SumStrictBookingAuditView(StrictBookingAuditView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedFte + parsedAmount + parsedScore;
    }
}
