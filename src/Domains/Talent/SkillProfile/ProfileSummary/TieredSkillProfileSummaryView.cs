namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered skill profile summary.</summary>
public sealed record TieredSkillProfileSummaryView(string Code, string TitleText, string BadgeText, string Name, string GraceDaysText, string Team, string WeightText, string SegmentText, string DescriptionText, string StatusLabel)
{
    public static TieredSkillProfileSummaryView ToTieredSkillProfileSummaryView(TieredSkillProfileSummary item)
    {
        return new TieredSkillProfileSummaryView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 120),
            item.Badge,
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSkillProfileSummaryView> ToTieredSkillProfileSummaryViews(List<TieredSkillProfileSummary> items)
    {
        var views = new List<TieredSkillProfileSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSkillProfileSummaryView(item));
        }
        return views;
    }

    public static double SumTieredSkillProfileSummaryView(TieredSkillProfileSummaryView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedWeight;
    }
}
