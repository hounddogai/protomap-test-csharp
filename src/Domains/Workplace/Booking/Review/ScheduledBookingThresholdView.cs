namespace Acme.Workforce.Workplace.Booking.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled booking threshold.</summary>
public sealed record ScheduledBookingThresholdView(string CodeText, string Label, string Reviewer, string Checksum, string CountryText, string GradeText, string BatchSize, string StatusLabel)
{
    public static ScheduledBookingThresholdView ToScheduledBookingThresholdView(ScheduledBookingThreshold item)
    {
        return new ScheduledBookingThresholdView(
            item.Code,
            item.Label,
            item.Reviewer,
            item.Checksum,
            Text.Truncate(item.Country, 80),
            item.Grade,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledBookingThresholdView> ToScheduledBookingThresholdViews(List<ScheduledBookingThreshold> items)
    {
        var views = new List<ScheduledBookingThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledBookingThresholdView(item));
        }
        return views;
    }

    public static double SumScheduledBookingThresholdView(ScheduledBookingThresholdView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
