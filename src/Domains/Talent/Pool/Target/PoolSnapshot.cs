namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool snapshot is in its life cycle.</summary>
public enum PoolSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pool snapshot. Its code is unique within a tenant.</summary>
public sealed class PoolSnapshot(string code, string title, string description, double amount, string skill, string employeeId, PoolSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public double Amount { get; set; } = amount;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public PoolSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a pool snapshot in status idle.</summary>
    public static PoolSnapshot NewPoolSnapshot(string code, string title)
    {
        return new PoolSnapshot(code, title, "", 0.0, "", "", PoolSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolSnapshotStatus next)
    {
        switch (Status)
        {
            case PoolSnapshotStatus.Idle:
                return next == PoolSnapshotStatus.Warming;
            case PoolSnapshotStatus.Warming:
                return next == PoolSnapshotStatus.Draining || next == PoolSnapshotStatus.Stopped;
            case PoolSnapshotStatus.Ready:
                return next == PoolSnapshotStatus.Draining || next == PoolSnapshotStatus.Stopped;
            case PoolSnapshotStatus.Draining:
                switch (next)
                {
                    case PoolSnapshotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pool snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PoolSnapshot Copy()
    {
        return new PoolSnapshot(Code, Title, Description, Amount, Skill, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
