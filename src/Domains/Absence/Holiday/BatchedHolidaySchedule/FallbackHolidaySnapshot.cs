namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback holiday snapshot is in its life cycle.</summary>
public enum FallbackHolidaySnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a fallback holiday snapshot and its current status.</summary>
public sealed class FallbackHolidaySnapshot(string code, string label, string externalRef, string employeeId, double minimum, int version, double ratio, FallbackHolidaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public int Version { get; set; } = version;
    public double Ratio { get; set; } = ratio;
    public FallbackHolidaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback holiday snapshot in status new.</summary>
    public static FallbackHolidaySnapshot NewFallbackHolidaySnapshot(string code, string label)
    {
        return new FallbackHolidaySnapshot(code, label, "", "", 0.0, 0, 0.0, FallbackHolidaySnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackHolidaySnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackHolidaySnapshotStatus next)
    {
        switch (Status)
        {
            case FallbackHolidaySnapshotStatus.New:
                switch (next)
                {
                    case FallbackHolidaySnapshotStatus.Done:
                        return true;
                }
                return false;
            case FallbackHolidaySnapshotStatus.Queued:
                return next == FallbackHolidaySnapshotStatus.Running || next == FallbackHolidaySnapshotStatus.Failed;
            case FallbackHolidaySnapshotStatus.Running:
                switch (next)
                {
                    case FallbackHolidaySnapshotStatus.Failed:
                        return true;
                }
                return false;
            case FallbackHolidaySnapshotStatus.Done:
                return next == FallbackHolidaySnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackHolidaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback holiday snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ratio + (double)Version, 2);
    }

    public FallbackHolidaySnapshot Copy()
    {
        return new FallbackHolidaySnapshot(Code, Label, ExternalRef, EmployeeId, Minimum, Version, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
