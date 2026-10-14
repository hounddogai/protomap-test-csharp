namespace Acme.Workforce.Compensation.Adjustment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment threshold.</summary>
public sealed record AdjustmentThresholdView(string Code, string Label, string Source, string JobCode, string Skill, string Active, string BadgeText, string LimitText, string StatusLabel)
{
    public static AdjustmentThresholdView ToAdjustmentThresholdView(AdjustmentThreshold item)
    {
        return new AdjustmentThresholdView(
            item.Code,
            item.Label,
            Text.Truncate(item.Source, 64),
            item.JobCode,
            item.Skill,
            $"{item.Active}",
            item.Badge,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentThresholdView> ToAdjustmentThresholdViews(List<AdjustmentThreshold> items)
    {
        var views = new List<AdjustmentThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentThresholdView(item));
        }
        return views;
    }

    public static double SumAdjustmentThresholdView(AdjustmentThresholdView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
