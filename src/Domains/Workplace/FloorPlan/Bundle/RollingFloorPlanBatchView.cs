namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling floor plan batch.</summary>
public sealed record RollingFloorPlanBatchView(string Code, string TitleText, string Skill, string GraceDays, string Sequence, string Version, string StatusLabel)
{
    public static RollingFloorPlanBatchView ToRollingFloorPlanBatchView(RollingFloorPlanBatch item)
    {
        return new RollingFloorPlanBatchView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Skill,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingFloorPlanBatchView> ToRollingFloorPlanBatchViews(List<RollingFloorPlanBatch> items)
    {
        var views = new List<RollingFloorPlanBatchView>();
        foreach (var item in items)
        {
            views.Add(ToRollingFloorPlanBatchView(item));
        }
        return views;
    }

    public static double SumRollingFloorPlanBatchView(RollingFloorPlanBatchView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedSequence + (double)parsedVersion;
    }
}
