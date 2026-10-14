namespace Acme.Workforce.Workplace.Booking.BookingPlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient booking plan.</summary>
public sealed record LenientBookingPlanView(string CodeText, string Name, string Attempts, string Visible, string WindowDays, string SegmentText, string Archived, string Score, string Amount, string StatusLabel)
{
    public static LenientBookingPlanView ToLenientBookingPlanView(LenientBookingPlan item)
    {
        return new LenientBookingPlanView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 64),
            $"{item.Archived}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBookingPlanView> ToLenientBookingPlanViews(List<LenientBookingPlan> items)
    {
        var views = new List<LenientBookingPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBookingPlanView(item));
        }
        return views;
    }

    public static double SumLenientBookingPlanView(LenientBookingPlanView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedWindowDays + parsedScore;
    }
}
