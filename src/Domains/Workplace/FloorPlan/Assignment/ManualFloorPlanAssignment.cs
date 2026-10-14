namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual floor plan assignment is in its life cycle.</summary>
public enum ManualFloorPlanAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual floor plan assignment as the floor plan team keeps it.</summary>
public sealed class ManualFloorPlanAssignment(string code, string name, List<string> tags, string reviewer, double score, int limit, string employeeId, double floorValue, int priority, ManualFloorPlanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public string EmployeeId { get; set; } = employeeId;
    public double FloorValue { get; set; } = floorValue;
    public int Priority { get; set; } = priority;
    public ManualFloorPlanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual floor plan assignment in status requested.</summary>
    public static ManualFloorPlanAssignment NewManualFloorPlanAssignment(string code, string name)
    {
        return new ManualFloorPlanAssignment(
            code,
            name,
            new List<string>(),
            "",
            0.0,
            0,
            "",
            0.0,
            0,
            ManualFloorPlanAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualFloorPlanAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualFloorPlanAssignmentStatus next)
    {
        switch (Status)
        {
            case ManualFloorPlanAssignmentStatus.Requested:
                switch (next)
                {
                    case ManualFloorPlanAssignmentStatus.Revoked:
                    case ManualFloorPlanAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case ManualFloorPlanAssignmentStatus.Granted:
                return next == ManualFloorPlanAssignmentStatus.Revoked || next == ManualFloorPlanAssignmentStatus.Expired;
            case ManualFloorPlanAssignmentStatus.Revoked:
                return next == ManualFloorPlanAssignmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualFloorPlanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual floor plan assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual floor plan assignment"} {Code}";
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

    public double NetExposure()
    {
        return Math.Round((double)Limit + (double)Priority, 2);
    }

    public ManualFloorPlanAssignment Copy()
    {
        return new ManualFloorPlanAssignment(
            Code,
            Name,
            Tags,
            Reviewer,
            Score,
            Limit,
            EmployeeId,
            FloorValue,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
