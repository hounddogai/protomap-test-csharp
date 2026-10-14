namespace Acme.Workforce.Compensation.Adjustment.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive adjustment target.</summary>
public sealed record AdaptiveAdjustmentTargetView(string Code, string LabelText, string RevisionText, string LeaveDays, string Limit, string RegionText, string UnitText, string StatusLabel)
{
    public static AdaptiveAdjustmentTargetView ToAdaptiveAdjustmentTargetView(AdaptiveAdjustmentTarget item)
    {
        return new AdaptiveAdjustmentTargetView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 80),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAdjustmentTargetView> ToAdaptiveAdjustmentTargetViews(List<AdaptiveAdjustmentTarget> items)
    {
        var views = new List<AdaptiveAdjustmentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAdjustmentTargetView(item));
        }
        return views;
    }

    public static double SumAdaptiveAdjustmentTargetView(AdaptiveAdjustmentTargetView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedLeaveDays + (double)parsedLimit;
    }
}
