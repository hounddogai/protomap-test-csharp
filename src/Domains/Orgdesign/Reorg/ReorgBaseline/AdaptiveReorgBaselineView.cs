namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive reorg baseline.</summary>
public sealed record AdaptiveReorgBaselineView(string Code, string NameText, string Timezone, string CategoryText, string LeaveDays, string FloorValueText, string UnitText, string Retries, string Pinned, string StatusLabel)
{
    public static AdaptiveReorgBaselineView ToAdaptiveReorgBaselineView(AdaptiveReorgBaseline item)
    {
        return new AdaptiveReorgBaselineView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Name, 120),
            item.Timezone,
            item.Category,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveReorgBaselineView> ToAdaptiveReorgBaselineViews(List<AdaptiveReorgBaseline> items)
    {
        var views = new List<AdaptiveReorgBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveReorgBaselineView(item));
        }
        return views;
    }

    public static double SumAdaptiveReorgBaselineView(AdaptiveReorgBaselineView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedFloorValue + (double)parsedRetries;
    }
}
