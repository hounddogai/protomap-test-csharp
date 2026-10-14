namespace Acme.Workforce.Engagement.Recognition.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition milestone.</summary>
public sealed record RecognitionMilestoneView(string CodeText, string Title, string RegionText, string Locale, string CreatedAt, string Limit, string Margin, string Score, string GraceDays, string StatusLabel)
{
    public static RecognitionMilestoneView ToRecognitionMilestoneView(RecognitionMilestone item)
    {
        return new RecognitionMilestoneView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Region,
            item.Locale,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionMilestoneView> ToRecognitionMilestoneViews(List<RecognitionMilestone> items)
    {
        var views = new List<RecognitionMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionMilestoneView(item));
        }
        return views;
    }

    public static double SumRecognitionMilestoneView(RecognitionMilestoneView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedLimit + parsedMargin;
    }
}
