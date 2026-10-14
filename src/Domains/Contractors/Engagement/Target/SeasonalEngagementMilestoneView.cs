namespace Acme.Workforce.Contractors.Engagement.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal engagement milestone.</summary>
public sealed record SeasonalEngagementMilestoneView(string CodeText, string LabelText, string TimezoneText, string Description, string TitleText, string Rating, string Summary, string SalaryBand, string CreatedAt, string StatusLabel)
{
    public static SeasonalEngagementMilestoneView ToSeasonalEngagementMilestoneView(SeasonalEngagementMilestone item)
    {
        return new SeasonalEngagementMilestoneView(
            item.Code,
            item.Label,
            Text.Truncate(item.Timezone, 80),
            Text.Truncate(item.Description, 64),
            item.Title,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.SalaryBand,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalEngagementMilestoneView> ToSeasonalEngagementMilestoneViews(List<SeasonalEngagementMilestone> items)
    {
        var views = new List<SeasonalEngagementMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalEngagementMilestoneView(item));
        }
        return views;
    }

    public static double SumSeasonalEngagementMilestoneView(SeasonalEngagementMilestoneView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedCreatedAt;
    }
}
