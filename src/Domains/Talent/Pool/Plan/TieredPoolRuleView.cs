namespace Acme.Workforce.Talent.Pool.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pool rule.</summary>
public sealed record TieredPoolRuleView(string Code, string Label, string Department, string Ceiling, string ChannelText, string Summary, string JobCode, string Region, string SegmentText, string UpdatedAt, string StatusLabel)
{
    public static TieredPoolRuleView ToTieredPoolRuleView(TieredPoolRule item)
    {
        return new TieredPoolRuleView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Department,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Channel, 64),
            item.Summary,
            item.JobCode,
            item.Region,
            item.Segment,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPoolRuleView> ToTieredPoolRuleViews(List<TieredPoolRule> items)
    {
        var views = new List<TieredPoolRuleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPoolRuleView(item));
        }
        return views;
    }

    public static double SumTieredPoolRuleView(TieredPoolRuleView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedUpdatedAt;
    }
}
