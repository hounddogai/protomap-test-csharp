namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict adjustment baseline.</summary>
public sealed record StrictAdjustmentBaselineView(string Code, string Name, string UpdatedAtText, string Version, string GraceDays, string Skill, string StatusLabel)
{
    public static StrictAdjustmentBaselineView ToStrictAdjustmentBaselineView(StrictAdjustmentBaseline item)
    {
        return new StrictAdjustmentBaselineView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAdjustmentBaselineView> ToStrictAdjustmentBaselineViews(List<StrictAdjustmentBaseline> items)
    {
        var views = new List<StrictAdjustmentBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAdjustmentBaselineView(item));
        }
        return views;
    }

    public static double SumStrictAdjustmentBaselineView(StrictAdjustmentBaselineView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedVersion + (double)parsedGraceDays;
    }
}
