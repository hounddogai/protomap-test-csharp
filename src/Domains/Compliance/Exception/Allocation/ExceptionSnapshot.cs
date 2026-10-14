namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception snapshot is in its life cycle.</summary>
public enum ExceptionSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A exception snapshot as the exception team keeps it.</summary>
public sealed class ExceptionSnapshot(string code, string label, string employeeId, int leaveDays, string phone, int createdAt, ExceptionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Phone { get; set; } = phone;
    public int CreatedAt { get; set; } = createdAt;
    public ExceptionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a exception snapshot in status idle.</summary>
    public static ExceptionSnapshot NewExceptionSnapshot(string code, string label)
    {
        return new ExceptionSnapshot(code, label, "", 0, "", 0, ExceptionSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionSnapshotStatus next)
    {
        switch (Status)
        {
            case ExceptionSnapshotStatus.Idle:
                return next == ExceptionSnapshotStatus.Warming;
            case ExceptionSnapshotStatus.Warming:
                return next == ExceptionSnapshotStatus.Ready;
            case ExceptionSnapshotStatus.Ready:
                switch (next)
                {
                    case ExceptionSnapshotStatus.Draining:
                    case ExceptionSnapshotStatus.Stopped:
                        return true;
                }
                return false;
            case ExceptionSnapshotStatus.Draining:
                switch (next)
                {
                    case ExceptionSnapshotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"exception snapshot"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)LeaveDays - (double)CreatedAt, 2);
    }

    public ExceptionSnapshot Copy()
    {
        return new ExceptionSnapshot(Code, Label, EmployeeId, LeaveDays, Phone, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
