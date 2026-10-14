namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a feedback quota.</summary>
public sealed record FeedbackQuotaView(string CodeText, string Name, string Region, string SalaryBand, string DurationMinutes, string Archived, string Ratio, string StatusLabel)
{
    public static FeedbackQuotaView ToFeedbackQuotaView(FeedbackQuota item)
    {
        return new FeedbackQuotaView(
            item.Code,
            item.Name,
            item.Region,
            item.SalaryBand,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackQuotaView> ToFeedbackQuotaViews(List<FeedbackQuota> items)
    {
        var views = new List<FeedbackQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackQuotaView(item));
        }
        return views;
    }

    public static double SumFeedbackQuotaView(FeedbackQuotaView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRatio;
    }
}
