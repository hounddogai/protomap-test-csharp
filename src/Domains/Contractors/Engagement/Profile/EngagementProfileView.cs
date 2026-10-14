namespace Acme.Workforce.Contractors.Engagement.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement profile.</summary>
public sealed record EngagementProfileView(string CodeText, string Name, string Source, string OwnerId, string CategoryText, string Score, string StatusLabel)
{
    public static EngagementProfileView ToEngagementProfileView(EngagementProfile item)
    {
        return new EngagementProfileView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Source,
            item.OwnerId,
            Text.Truncate(item.Category, 40),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementProfileView> ToEngagementProfileViews(List<EngagementProfile> items)
    {
        var views = new List<EngagementProfileView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementProfileView(item));
        }
        return views;
    }

    public static double SumEngagementProfileView(EngagementProfileView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
