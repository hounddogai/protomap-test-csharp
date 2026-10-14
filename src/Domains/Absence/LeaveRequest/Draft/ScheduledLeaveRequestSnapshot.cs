namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request snapshot is in its life cycle.</summary>
public enum ScheduledLeaveRequestSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled leave request snapshot as the leave request team keeps it.</summary>
public sealed class ScheduledLeaveRequestSnapshot(string code, string name, int priority, int endsAt, string title, int createdAt, int windowDays, double ratio, double hours, ScheduledLeaveRequestSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public double Ratio { get; set; } = ratio;
    public double Hours { get; set; } = hours;
    public ScheduledLeaveRequestSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request snapshot in status idle.</summary>
    public static ScheduledLeaveRequestSnapshot NewScheduledLeaveRequestSnapshot(string code, string name)
    {
        return new ScheduledLeaveRequestSnapshot(
            code,
            name,
            0,
            0,
            "",
            0,
            0,
            0.0,
            0.0,
            ScheduledLeaveRequestSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestSnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestSnapshotStatus.Idle:
                return next == ScheduledLeaveRequestSnapshotStatus.Warming;
            case ScheduledLeaveRequestSnapshotStatus.Warming:
                return next == ScheduledLeaveRequestSnapshotStatus.Ready || next == ScheduledLeaveRequestSnapshotStatus.Draining;
            case ScheduledLeaveRequestSnapshotStatus.Ready:
                return next == ScheduledLeaveRequestSnapshotStatus.Draining || next == ScheduledLeaveRequestSnapshotStatus.Stopped;
            case ScheduledLeaveRequestSnapshotStatus.Draining:
                return next == ScheduledLeaveRequestSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled leave request snapshot"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Priority - (double)EndsAt, 2);
    }

    public ScheduledLeaveRequestSnapshot Copy()
    {
        return new ScheduledLeaveRequestSnapshot(
            Code,
            Name,
            Priority,
            EndsAt,
            Title,
            CreatedAt,
            WindowDays,
            Ratio,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
