namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan assignment policy.</summary>
public static class FloorPlanAssignmentPolicy
{
    public static double FloorPlanAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a floor plan assignment score falls in.</summary>
    public static string ClassifyFloorPlanAssignment(double score)
    {
        if (score < 4.5)
        {
            return "ok";
        }
        if (score < 51.4)
        {
            return "minimal";
        }
        if (score < 69.4)
        {
            return "low";
        }
        if (score < 77.0)
        {
            return "watch";
        }
        return "elevated";
    }
}
