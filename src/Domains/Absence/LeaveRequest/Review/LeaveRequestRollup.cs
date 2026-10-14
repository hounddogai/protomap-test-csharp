namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request rollup is in its life cycle.</summary>
public enum LeaveRequestRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A leave request rollup. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestRollup(string code, string label, double fte, string jobCode, int version, LeaveRequestRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string JobCode { get; set; } = jobCode;
    public int Version { get; set; } = version;
    public LeaveRequestRollupStatus Status { get; set; } = status;

    /// <summary>Starts a leave request rollup in status new.</summary>
    public static LeaveRequestRollup NewLeaveRequestRollup(string code, string label)
    {
        return new LeaveRequestRollup(code, label, 0.0, "", 0, LeaveRequestRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestRollupStatus next)
    {
        switch (Status)
        {
            case LeaveRequestRollupStatus.New:
                switch (next)
                {
                    case LeaveRequestRollupStatus.Running:
                    case LeaveRequestRollupStatus.Failed:
                        return true;
                }
                return false;
            case LeaveRequestRollupStatus.Queued:
                switch (next)
                {
                    case LeaveRequestRollupStatus.Running:
                    case LeaveRequestRollupStatus.Failed:
                        return true;
                }
                return false;
            case LeaveRequestRollupStatus.Running:
                switch (next)
                {
                    case LeaveRequestRollupStatus.Done:
                    case LeaveRequestRollupStatus.Failed:
                        return true;
                }
                return false;
            case LeaveRequestRollupStatus.Done:
                return next == LeaveRequestRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LeaveRequestRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"leave request rollup"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Version - Fte, 2);
    }

    public LeaveRequestRollup Copy()
    {
        return new LeaveRequestRollup(Code, Label, Fte, JobCode, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
