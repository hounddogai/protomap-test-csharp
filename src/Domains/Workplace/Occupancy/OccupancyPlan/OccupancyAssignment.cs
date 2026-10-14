namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy assignment is in its life cycle.</summary>
public enum OccupancyAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One occupancy assignment, with what the planners need to act on it.</summary>
public sealed class OccupancyAssignment(string code, string name, string notes, string unit, string reviewer, int durationMinutes, string jobCode, int retries, OccupancyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public string Unit { get; set; } = unit;
    public string Reviewer { get; set; } = reviewer;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string JobCode { get; set; } = jobCode;
    public int Retries { get; set; } = retries;
    public OccupancyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy assignment in status open.</summary>
    public static OccupancyAssignment NewOccupancyAssignment(string code, string name)
    {
        return new OccupancyAssignment(code, name, "", "", "", 0, "", 0, OccupancyAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyAssignmentStatus next)
    {
        switch (Status)
        {
            case OccupancyAssignmentStatus.Open:
                switch (next)
                {
                    case OccupancyAssignmentStatus.Resolved:
                    case OccupancyAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyAssignmentStatus.InReview:
                switch (next)
                {
                    case OccupancyAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case OccupancyAssignmentStatus.Resolved:
                switch (next)
                {
                    case OccupancyAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case OccupancyAssignmentStatus.Reopened:
                switch (next)
                {
                    case OccupancyAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries - (double)DurationMinutes, 2);
    }

    public OccupancyAssignment Copy()
    {
        return new OccupancyAssignment(Code, Name, Notes, Unit, Reviewer, DurationMinutes, JobCode, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
