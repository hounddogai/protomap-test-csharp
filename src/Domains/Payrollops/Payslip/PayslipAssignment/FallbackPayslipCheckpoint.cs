namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback payslip checkpoint is in its life cycle.</summary>
public enum FallbackPayslipCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a fallback payslip checkpoint and its current status.</summary>
public sealed class FallbackPayslipCheckpoint(string code, string name, int attempts, double amount, string currency, string department, string summary, int batchSize, FallbackPayslipCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public string Department { get; set; } = department;
    public string Summary { get; set; } = summary;
    public int BatchSize { get; set; } = batchSize;
    public FallbackPayslipCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a fallback payslip checkpoint in status proposed.</summary>
    public static FallbackPayslipCheckpoint NewFallbackPayslipCheckpoint(string code, string name)
    {
        return new FallbackPayslipCheckpoint(
            code,
            name,
            0,
            0.0,
            "",
            "",
            "",
            0,
            FallbackPayslipCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayslipCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayslipCheckpointStatus next)
    {
        switch (Status)
        {
            case FallbackPayslipCheckpointStatus.Proposed:
                return next == FallbackPayslipCheckpointStatus.Approved;
            case FallbackPayslipCheckpointStatus.Approved:
                switch (next)
                {
                    case FallbackPayslipCheckpointStatus.Rejected:
                        return true;
                }
                return false;
            case FallbackPayslipCheckpointStatus.Rejected:
                return next == FallbackPayslipCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackPayslipCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback payslip checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Attempts + Amount, 2);
    }

    public FallbackPayslipCheckpoint Copy()
    {
        return new FallbackPayslipCheckpoint(
            Code,
            Name,
            Attempts,
            Amount,
            Currency,
            Department,
            Summary,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
