namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling accrual assignment is in its life cycle.</summary>
public enum RollingAccrualAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rolling accrual assignment as the accrual team keeps it.</summary>
public sealed class RollingAccrualAssignment(string code, string title, string unit, int batchSize, int revision, RollingAccrualAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public int BatchSize { get; set; } = batchSize;
    public int Revision { get; set; } = revision;
    public RollingAccrualAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling accrual assignment in status proposed.</summary>
    public static RollingAccrualAssignment NewRollingAccrualAssignment(string code, string title)
    {
        return new RollingAccrualAssignment(code, title, "", 0, 0, RollingAccrualAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccrualAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccrualAssignmentStatus next)
    {
        switch (Status)
        {
            case RollingAccrualAssignmentStatus.Proposed:
                switch (next)
                {
                    case RollingAccrualAssignmentStatus.Approved:
                    case RollingAccrualAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingAccrualAssignmentStatus.Approved:
                switch (next)
                {
                    case RollingAccrualAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case RollingAccrualAssignmentStatus.Rejected:
                return next == RollingAccrualAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingAccrualAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling accrual assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)BatchSize * (double)Revision, 2);
    }

    public RollingAccrualAssignment Copy()
    {
        return new RollingAccrualAssignment(Code, Title, Unit, BatchSize, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
