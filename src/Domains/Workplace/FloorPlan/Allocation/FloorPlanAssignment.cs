namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan assignment is in its life cycle.</summary>
public enum FloorPlanAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One floor plan assignment, with what the planners need to act on it.</summary>
public sealed class FloorPlanAssignment(string code, string title, List<string> tags, int windowDays, double weight, double floorValue, bool archived, string description, string category, int endsAt, FloorPlanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public string Category { get; set; } = category;
    public int EndsAt { get; set; } = endsAt;
    public FloorPlanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan assignment in status planned.</summary>
    public static FloorPlanAssignment NewFloorPlanAssignment(string code, string title)
    {
        return new FloorPlanAssignment(
            code,
            title,
            new List<string>(),
            0,
            0.0,
            0.0,
            false,
            "",
            "",
            0,
            FloorPlanAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanAssignmentStatus next)
    {
        switch (Status)
        {
            case FloorPlanAssignmentStatus.Planned:
                return next == FloorPlanAssignmentStatus.Completed || next == FloorPlanAssignmentStatus.Cancelled;
            case FloorPlanAssignmentStatus.Scheduled:
                return next == FloorPlanAssignmentStatus.InProgress || next == FloorPlanAssignmentStatus.Cancelled;
            case FloorPlanAssignmentStatus.InProgress:
                return next == FloorPlanAssignmentStatus.Completed || next == FloorPlanAssignmentStatus.Cancelled;
            case FloorPlanAssignmentStatus.Completed:
                switch (next)
                {
                    case FloorPlanAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"floor plan assignment"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)EndsAt * (double)WindowDays, 2);
    }

    public FloorPlanAssignment Copy()
    {
        return new FloorPlanAssignment(
            Code,
            Title,
            Tags,
            WindowDays,
            Weight,
            FloorValue,
            Archived,
            Description,
            Category,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
