namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default floor plan target is in its life cycle.</summary>
public enum DefaultFloorPlanTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default floor plan target. Its code is unique within a tenant.</summary>
public sealed class DefaultFloorPlanTarget(string code, string label, string name, bool approved, int startsAt, bool archived, string reviewer, int sequence, DefaultFloorPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public int Sequence { get; set; } = sequence;
    public DefaultFloorPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a default floor plan target in status planned.</summary>
    public static DefaultFloorPlanTarget NewDefaultFloorPlanTarget(string code, string label)
    {
        return new DefaultFloorPlanTarget(
            code,
            label,
            "",
            false,
            0,
            false,
            "",
            0,
            DefaultFloorPlanTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultFloorPlanTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultFloorPlanTargetStatus next)
    {
        switch (Status)
        {
            case DefaultFloorPlanTargetStatus.Planned:
                return next == DefaultFloorPlanTargetStatus.Completed;
            case DefaultFloorPlanTargetStatus.Scheduled:
                return next == DefaultFloorPlanTargetStatus.InProgress;
            case DefaultFloorPlanTargetStatus.InProgress:
                switch (next)
                {
                    case DefaultFloorPlanTargetStatus.Completed:
                        return true;
                }
                return false;
            case DefaultFloorPlanTargetStatus.Completed:
                switch (next)
                {
                    case DefaultFloorPlanTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultFloorPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default floor plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"default floor plan target"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt * (double)Sequence, 2);
    }

    public DefaultFloorPlanTarget Copy()
    {
        return new DefaultFloorPlanTarget(
            Code,
            Label,
            Name,
            Approved,
            StartsAt,
            Archived,
            Reviewer,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
