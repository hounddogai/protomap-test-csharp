namespace Acme.Workforce.Workplace.Booking.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled booking claim.</summary>
public sealed record ScheduledBookingClaimView(string Code, string Title, string RateText, string Attempts, string PinnedText, string Retries, string Category, string StatusLabel)
{
    public static ScheduledBookingClaimView ToScheduledBookingClaimView(ScheduledBookingClaim item)
    {
        return new ScheduledBookingClaimView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBookingClaimView> ToScheduledBookingClaimViews(List<ScheduledBookingClaim> items)
    {
        var views = new List<ScheduledBookingClaimView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBookingClaimView(item));
        }
        return views;
    }

    public static double SumScheduledBookingClaimView(ScheduledBookingClaimView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedAttempts + (double)parsedRetries;
    }
}
