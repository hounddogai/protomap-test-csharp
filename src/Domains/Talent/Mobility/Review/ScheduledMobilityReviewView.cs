namespace Acme.Workforce.Talent.Mobility.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled mobility review.</summary>
public sealed record ScheduledMobilityReviewView(string CodeText, string TitleText, string MaximumText, string Channel, string Location, string ExternalRef, string DurationMinutes, string StatusLabel)
{
    public static ScheduledMobilityReviewView ToScheduledMobilityReviewView(ScheduledMobilityReview item)
    {
        return new ScheduledMobilityReviewView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Channel,
            Text.Truncate(item.Location, 80),
            item.ExternalRef,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledMobilityReviewView> ToScheduledMobilityReviewViews(List<ScheduledMobilityReview> items)
    {
        var views = new List<ScheduledMobilityReviewView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledMobilityReviewView(item));
        }
        return views;
    }

    public static double SumScheduledMobilityReviewView(ScheduledMobilityReviewView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedDurationMinutes;
    }
}
