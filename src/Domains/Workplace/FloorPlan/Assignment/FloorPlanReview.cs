namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan review is in its life cycle.</summary>
public enum FloorPlanReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A floor plan review as the floor plan team keeps it.</summary>
public sealed class FloorPlanReview(string code, string name, double ratio, double threshold, double ceiling, string category, FloorPlanReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public double Threshold { get; set; } = threshold;
    public double Ceiling { get; set; } = ceiling;
    public string Category { get; set; } = category;
    public FloorPlanReviewStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan review in status proposed.</summary>
    public static FloorPlanReview NewFloorPlanReview(string code, string name)
    {
        return new FloorPlanReview(code, name, 0.0, 0.0, 0.0, "", FloorPlanReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanReviewStatus next)
    {
        switch (Status)
        {
            case FloorPlanReviewStatus.Proposed:
                switch (next)
                {
                    case FloorPlanReviewStatus.Approved:
                        return true;
                }
                return false;
            case FloorPlanReviewStatus.Approved:
                switch (next)
                {
                    case FloorPlanReviewStatus.Withdrawn:
                        return true;
                }
                return false;
            case FloorPlanReviewStatus.Rejected:
                return next == FloorPlanReviewStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FloorPlanReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"floor plan review"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Ceiling + Threshold, 2);
    }

    public FloorPlanReview Copy()
    {
        return new FloorPlanReview(Code, Name, Ratio, Threshold, Ceiling, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
