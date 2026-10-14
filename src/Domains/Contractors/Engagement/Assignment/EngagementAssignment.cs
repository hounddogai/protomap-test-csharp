namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement assignment is in its life cycle.</summary>
public enum EngagementAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A engagement assignment. Its code is unique within a tenant.</summary>
public sealed class EngagementAssignment(string code, string title, int startsAt, string name, string source, int durationMinutes, EngagementAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public int DurationMinutes { get; set; } = durationMinutes;
    public EngagementAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a engagement assignment in status planned.</summary>
    public static EngagementAssignment NewEngagementAssignment(string code, string title)
    {
        return new EngagementAssignment(code, title, 0, "", "", 0, EngagementAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementAssignmentStatus next)
    {
        switch (Status)
        {
            case EngagementAssignmentStatus.Planned:
                return next == EngagementAssignmentStatus.Scheduled || next == EngagementAssignmentStatus.Completed;
            case EngagementAssignmentStatus.Scheduled:
                switch (next)
                {
                    case EngagementAssignmentStatus.InProgress:
                    case EngagementAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case EngagementAssignmentStatus.InProgress:
                return next == EngagementAssignmentStatus.Completed || next == EngagementAssignmentStatus.Cancelled;
            case EngagementAssignmentStatus.Completed:
                return next == EngagementAssignmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EngagementAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)DurationMinutes - (double)StartsAt, 2);
    }

    public EngagementAssignment Copy()
    {
        return new EngagementAssignment(Code, Title, StartsAt, Name, Source, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
