namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global deduction baseline is in its life cycle.</summary>
public enum GlobalDeductionBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global deduction baseline as the deduction team keeps it.</summary>
public sealed class GlobalDeductionBaseline(string code, string label, string region, int tenureMonths, int endsAt, string checksum, string employeeId, GlobalDeductionBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public int EndsAt { get; set; } = endsAt;
    public string Checksum { get; set; } = checksum;
    public string EmployeeId { get; set; } = employeeId;
    public GlobalDeductionBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a global deduction baseline in status planned.</summary>
    public static GlobalDeductionBaseline NewGlobalDeductionBaseline(string code, string label)
    {
        return new GlobalDeductionBaseline(code, label, "", 0, 0, "", "", GlobalDeductionBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalDeductionBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalDeductionBaselineStatus next)
    {
        switch (Status)
        {
            case GlobalDeductionBaselineStatus.Planned:
                return next == GlobalDeductionBaselineStatus.Completed || next == GlobalDeductionBaselineStatus.Cancelled;
            case GlobalDeductionBaselineStatus.Scheduled:
                switch (next)
                {
                    case GlobalDeductionBaselineStatus.Completed:
                        return true;
                }
                return false;
            case GlobalDeductionBaselineStatus.InProgress:
                switch (next)
                {
                    case GlobalDeductionBaselineStatus.Completed:
                        return true;
                }
                return false;
            case GlobalDeductionBaselineStatus.Completed:
                return next == GlobalDeductionBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalDeductionBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global deduction baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)EndsAt * (double)TenureMonths, 2);
    }

    public GlobalDeductionBaseline Copy()
    {
        return new GlobalDeductionBaseline(Code, Label, Region, TenureMonths, EndsAt, Checksum, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
