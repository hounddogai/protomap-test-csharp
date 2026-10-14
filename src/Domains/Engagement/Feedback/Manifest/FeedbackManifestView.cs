namespace Acme.Workforce.Engagement.Feedback.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback manifest.</summary>
public sealed record FeedbackManifestView(string Code, string Name, string Cost, string Skill, string GradeText, string Approved, string Description, string StatusLabel)
{
    public static FeedbackManifestView ToFeedbackManifestView(FeedbackManifest item)
    {
        return new FeedbackManifestView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Grade,
            $"{item.Approved}",
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackManifestView> ToFeedbackManifestViews(List<FeedbackManifest> items)
    {
        var views = new List<FeedbackManifestView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackManifestView(item));
        }
        return views;
    }

    public static double SumFeedbackManifestView(FeedbackManifestView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
