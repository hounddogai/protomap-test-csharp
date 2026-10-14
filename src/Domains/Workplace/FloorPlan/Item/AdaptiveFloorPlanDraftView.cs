namespace Acme.Workforce.Workplace.FloorPlan.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive floor plan draft.</summary>
public sealed record AdaptiveFloorPlanDraftView(string Code, string NameText, string LimitText, string ChecksumText, string Minimum, string StatusLabel)
{
    public static AdaptiveFloorPlanDraftView ToAdaptiveFloorPlanDraftView(AdaptiveFloorPlanDraft item)
    {
        return new AdaptiveFloorPlanDraftView(
            item.Code,
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveFloorPlanDraftView> ToAdaptiveFloorPlanDraftViews(List<AdaptiveFloorPlanDraft> items)
    {
        var views = new List<AdaptiveFloorPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveFloorPlanDraftView(item));
        }
        return views;
    }

    public static double SumAdaptiveFloorPlanDraftView(AdaptiveFloorPlanDraftView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMinimum;
    }
}
