namespace Acme.Workforce.Contractors.Engagement.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global engagement score.</summary>
public sealed record GlobalEngagementScoreView(string CodeText, string Title, string Maximum, string EndsAt, string CreatedAt, string ContactEmail, string StatusLabel)
{
    public static GlobalEngagementScoreView ToGlobalEngagementScoreView(GlobalEngagementScore item)
    {
        return new GlobalEngagementScoreView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalEngagementScoreView> ToGlobalEngagementScoreViews(List<GlobalEngagementScore> items)
    {
        var views = new List<GlobalEngagementScoreView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalEngagementScoreView(item));
        }
        return views;
    }

    public static double SumGlobalEngagementScoreView(GlobalEngagementScoreView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedEndsAt + (double)parsedCreatedAt;
    }
}
