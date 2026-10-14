namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position assignment is in its life cycle.</summary>
public enum PositionAssignmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A position assignment as the position team keeps it.</summary>
public sealed class PositionAssignment(string code, string title, string employeeId, bool visible, string contactEmail, double weight, int capacity, double score, bool active, PositionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public double Score { get; set; } = score;
    public bool Active { get; set; } = active;
    public PositionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a position assignment in status new.</summary>
    public static PositionAssignment NewPositionAssignment(string code, string title)
    {
        return new PositionAssignment(code, title, "", false, "", 0.0, 0, 0.0, false, PositionAssignmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionAssignmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionAssignmentStatus next)
    {
        switch (Status)
        {
            case PositionAssignmentStatus.New:
                switch (next)
                {
                    case PositionAssignmentStatus.Running:
                    case PositionAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case PositionAssignmentStatus.Queued:
                switch (next)
                {
                    case PositionAssignmentStatus.Running:
                    case PositionAssignmentStatus.Done:
                        return true;
                }
                return false;
            case PositionAssignmentStatus.Running:
                switch (next)
                {
                    case PositionAssignmentStatus.Done:
                    case PositionAssignmentStatus.Failed:
                        return true;
                }
                return false;
            case PositionAssignmentStatus.Done:
                return next == PositionAssignmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PositionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Capacity * Score, 2);
    }

    public PositionAssignment Copy()
    {
        return new PositionAssignment(
            Code,
            Title,
            EmployeeId,
            Visible,
            ContactEmail,
            Weight,
            Capacity,
            Score,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
