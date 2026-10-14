namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using System.Globalization;

/// <summary>How screens and exports show a adjustment milestone.</summary>
public sealed record AdjustmentMilestoneView(string Code, string Label, string Fte, string RatioText, string Attempts, string Threshold, string StatusLabel)
{
    public static AdjustmentMilestoneView ToAdjustmentMilestoneView(AdjustmentMilestone item)
    {
        return new AdjustmentMilestoneView(
            item.Code,
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentMilestoneView> ToAdjustmentMilestoneViews(List<AdjustmentMilestone> items)
    {
        var views = new List<AdjustmentMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentMilestoneView(item));
        }
        return views;
    }

    public static double SumAdjustmentMilestoneView(AdjustmentMilestoneView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedFte + parsedRatio + (double)parsedAttempts;
    }
}
