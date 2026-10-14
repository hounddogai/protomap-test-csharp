namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual floor plan reservation policy.</summary>
public static class ManualFloorPlanReservationPolicy
{
    public static List<string> MergeManualFloorPlanReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ManualFloorPlanReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
