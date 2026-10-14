namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched payslip snapshot is in its life cycle.</summary>
public enum BatchedPayslipSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched payslip snapshot as the payslip team keeps it.</summary>
public sealed class BatchedPayslipSnapshot(string code, string name, string salaryBand, double hours, int createdAt, string region, BatchedPayslipSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Hours { get; set; } = hours;
    public int CreatedAt { get; set; } = createdAt;
    public string Region { get; set; } = region;
    public BatchedPayslipSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched payslip snapshot in status proposed.</summary>
    public static BatchedPayslipSnapshot NewBatchedPayslipSnapshot(string code, string name)
    {
        return new BatchedPayslipSnapshot(code, name, "", 0.0, 0, "", BatchedPayslipSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPayslipSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPayslipSnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedPayslipSnapshotStatus.Proposed:
                switch (next)
                {
                    case BatchedPayslipSnapshotStatus.Approved:
                        return true;
                }
                return false;
            case BatchedPayslipSnapshotStatus.Approved:
                switch (next)
                {
                    case BatchedPayslipSnapshotStatus.Rejected:
                    case BatchedPayslipSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedPayslipSnapshotStatus.Rejected:
                switch (next)
                {
                    case BatchedPayslipSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPayslipSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched payslip snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)CreatedAt * Hours, 2);
    }

    public BatchedPayslipSnapshot Copy()
    {
        return new BatchedPayslipSnapshot(Code, Name, SalaryBand, Hours, CreatedAt, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
