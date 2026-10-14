namespace Acme.Workforce.Compensation.Adjustment.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment plan.</summary>
public sealed record AdjustmentPlanView(string Code, string Label, string Skill, string Ratio, string Ceiling, string Capacity, string JobCodeText, string Discount, string StatusLabel)
{
    public static AdjustmentPlanView ToAdjustmentPlanView(AdjustmentPlan item)
    {
        return new AdjustmentPlanView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Skill, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentPlanView> ToAdjustmentPlanViews(List<AdjustmentPlan> items)
    {
        var views = new List<AdjustmentPlanView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentPlanView(item));
        }
        return views;
    }

    public static double SumAdjustmentPlanView(AdjustmentPlanView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCeiling + (double)parsedCapacity;
    }
}
