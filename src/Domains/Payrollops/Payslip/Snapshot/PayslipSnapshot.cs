namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip snapshot is in its life cycle.</summary>
public enum PayslipSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One payslip snapshot, with what the planners need to act on it.</summary>
public sealed class PayslipSnapshot(string code, string label, int sequence, double floorValue, int durationMinutes, string department, PayslipSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public double FloorValue { get; set; } = floorValue;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public PayslipSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a payslip snapshot in status new.</summary>
    public static PayslipSnapshot NewPayslipSnapshot(string code, string label)
    {
        return new PayslipSnapshot(code, label, 0, 0.0, 0, "", PayslipSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipSnapshotStatus next)
    {
        switch (Status)
        {
            case PayslipSnapshotStatus.New:
                return next == PayslipSnapshotStatus.Queued;
            case PayslipSnapshotStatus.Queued:
                return next == PayslipSnapshotStatus.Running || next == PayslipSnapshotStatus.Done;
            case PayslipSnapshotStatus.Running:
                switch (next)
                {
                    case PayslipSnapshotStatus.Done:
                        return true;
                }
                return false;
            case PayslipSnapshotStatus.Done:
                switch (next)
                {
                    case PayslipSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"payslip snapshot"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue - (double)DurationMinutes, 2);
    }

    public PayslipSnapshot Copy()
    {
        return new PayslipSnapshot(Code, Label, Sequence, FloorValue, DurationMinutes, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
