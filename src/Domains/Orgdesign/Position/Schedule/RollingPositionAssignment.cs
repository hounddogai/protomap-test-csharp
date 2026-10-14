namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling position assignment is in its life cycle.</summary>
public enum RollingPositionAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rolling position assignment and its current status.</summary>
public sealed class RollingPositionAssignment(string code, string title, string team, double fte, double minimum, int capacity, string label, string employeeId, string category, int retries, bool active, RollingPositionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public double Fte { get; set; } = fte;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Category { get; set; } = category;
    public int Retries { get; set; } = retries;
    public bool Active { get; set; } = active;
    public RollingPositionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling position assignment in status new.</summary>
    public static RollingPositionAssignment NewRollingPositionAssignment(string code, string title)
    {
        return new RollingPositionAssignment(
            code,
            title,
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            "",
            0,
            false,
            RollingPositionAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPositionAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPositionAssignmentStatus next)
    {
        switch (Status)
        {
            case RollingPositionAssignmentStatus.New:
                switch (next)
                {
                    case RollingPositionAssignmentStatus.Queued:
                        return true;
                }
                return false;
            case RollingPositionAssignmentStatus.Queued:
                switch (next)
                {
                    case RollingPositionAssignmentStatus.Done:
                        return true;
                }
                return false;
            case RollingPositionAssignmentStatus.Running:
                return next == RollingPositionAssignmentStatus.Done || next == RollingPositionAssignmentStatus.Failed;
            case RollingPositionAssignmentStatus.Done:
                switch (next)
                {
                    case RollingPositionAssignmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPositionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling position assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum - Fte, 2);
    }

    public RollingPositionAssignment Copy()
    {
        return new RollingPositionAssignment(
            Code,
            Title,
            Team,
            Fte,
            Minimum,
            Capacity,
            Label,
            EmployeeId,
            Category,
            Retries,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
