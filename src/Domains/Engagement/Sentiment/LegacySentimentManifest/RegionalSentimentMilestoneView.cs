namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional sentiment milestone.</summary>
public sealed record RegionalSentimentMilestoneView(string Code, string Label, string PriorityText, string ContactEmail, string Attempts, string StatusLabel)
{
    public static RegionalSentimentMilestoneView ToRegionalSentimentMilestoneView(RegionalSentimentMilestone item)
    {
        return new RegionalSentimentMilestoneView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSentimentMilestoneView> ToRegionalSentimentMilestoneViews(List<RegionalSentimentMilestone> items)
    {
        var views = new List<RegionalSentimentMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSentimentMilestoneView(item));
        }
        return views;
    }

    public static double SumRegionalSentimentMilestoneView(RegionalSentimentMilestoneView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedAttempts;
    }
}
