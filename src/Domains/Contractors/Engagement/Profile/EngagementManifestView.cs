namespace Acme.Workforce.Contractors.Engagement.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement manifest.</summary>
public sealed record EngagementManifestView(string CodeText, string Title, string ChannelText, string Grade, string ScoreText, string StatusLabel)
{
    public static EngagementManifestView ToEngagementManifestView(EngagementManifest item)
    {
        return new EngagementManifestView(
            item.Code,
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Channel, 120),
            item.Grade,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementManifestView> ToEngagementManifestViews(List<EngagementManifest> items)
    {
        var views = new List<EngagementManifestView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementManifestView(item));
        }
        return views;
    }

    public static double SumEngagementManifestView(EngagementManifestView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
