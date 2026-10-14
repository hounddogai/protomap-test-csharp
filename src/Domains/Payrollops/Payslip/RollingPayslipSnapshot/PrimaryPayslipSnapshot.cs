namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary payslip snapshot is in its life cycle.</summary>
public enum PrimaryPayslipSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One primary payslip snapshot, with what the planners need to act on it.</summary>
public sealed class PrimaryPayslipSnapshot(string code, string label, string location, string team, int durationMinutes, string phone, string employeeId, string category, string locale, int graceDays, PrimaryPayslipSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string Team { get; set; } = team;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Phone { get; set; } = phone;
    public string EmployeeId { get; set; } = employeeId;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public int GraceDays { get; set; } = graceDays;
    public PrimaryPayslipSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a primary payslip snapshot in status open.</summary>
    public static PrimaryPayslipSnapshot NewPrimaryPayslipSnapshot(string code, string label)
    {
        return new PrimaryPayslipSnapshot(
            code,
            label,
            "",
            "",
            0,
            "",
            "",
            "",
            "",
            0,
            PrimaryPayslipSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPayslipSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPayslipSnapshotStatus next)
    {
        switch (Status)
        {
            case PrimaryPayslipSnapshotStatus.Open:
                switch (next)
                {
                    case PrimaryPayslipSnapshotStatus.InReview:
                    case PrimaryPayslipSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryPayslipSnapshotStatus.InReview:
                switch (next)
                {
                    case PrimaryPayslipSnapshotStatus.Reopened:
                    case PrimaryPayslipSnapshotStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryPayslipSnapshotStatus.Resolved:
                return next == PrimaryPayslipSnapshotStatus.Reopened || next == PrimaryPayslipSnapshotStatus.Archived;
            case PrimaryPayslipSnapshotStatus.Reopened:
                return next == PrimaryPayslipSnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryPayslipSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary payslip snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)GraceDays - (double)DurationMinutes, 2);
    }

    public PrimaryPayslipSnapshot Copy()
    {
        return new PrimaryPayslipSnapshot(
            Code,
            Label,
            Location,
            Team,
            DurationMinutes,
            Phone,
            EmployeeId,
            Category,
            Locale,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
