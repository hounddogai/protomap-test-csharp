namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled floor plan snapshot is in its life cycle.</summary>
public enum ScheduledFloorPlanSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled floor plan snapshot. Its code is unique within a tenant.</summary>
public sealed class ScheduledFloorPlanSnapshot(string code, string title, int version, string salaryBand, int windowDays, double ceiling, string employeeId, ScheduledFloorPlanSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string SalaryBand { get; set; } = salaryBand;
    public int WindowDays { get; set; } = windowDays;
    public double Ceiling { get; set; } = ceiling;
    public string EmployeeId { get; set; } = employeeId;
    public ScheduledFloorPlanSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled floor plan snapshot in status new.</summary>
    public static ScheduledFloorPlanSnapshot NewScheduledFloorPlanSnapshot(string code, string title)
    {
        return new ScheduledFloorPlanSnapshot(code, title, 0, "", 0, 0.0, "", ScheduledFloorPlanSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFloorPlanSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFloorPlanSnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledFloorPlanSnapshotStatus.New:
                return next == ScheduledFloorPlanSnapshotStatus.Queued || next == ScheduledFloorPlanSnapshotStatus.Running;
            case ScheduledFloorPlanSnapshotStatus.Queued:
                switch (next)
                {
                    case ScheduledFloorPlanSnapshotStatus.Done:
                    case ScheduledFloorPlanSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledFloorPlanSnapshotStatus.Running:
                return next == ScheduledFloorPlanSnapshotStatus.Failed;
            case ScheduledFloorPlanSnapshotStatus.Done:
                return next == ScheduledFloorPlanSnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledFloorPlanSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled floor plan snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Version * (double)WindowDays, 2);
    }

    public ScheduledFloorPlanSnapshot Copy()
    {
        return new ScheduledFloorPlanSnapshot(
            Code,
            Title,
            Version,
            SalaryBand,
            WindowDays,
            Ceiling,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
