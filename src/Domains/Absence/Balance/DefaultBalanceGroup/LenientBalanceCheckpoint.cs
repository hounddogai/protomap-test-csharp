namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient balance checkpoint is in its life cycle.</summary>
public enum LenientBalanceCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient balance checkpoint. Its code is unique within a tenant.</summary>
public sealed class LenientBalanceCheckpoint(string code, string title, double hours, string currency, double ceiling, double discount, string employeeId, double minimum, string country, string contactEmail, string salaryBand, LenientBalanceCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Currency { get; set; } = currency;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public string ContactEmail { get; set; } = contactEmail;
    public string SalaryBand { get; set; } = salaryBand;
    public LenientBalanceCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a lenient balance checkpoint in status proposed.</summary>
    public static LenientBalanceCheckpoint NewLenientBalanceCheckpoint(string code, string title)
    {
        return new LenientBalanceCheckpoint(
            code,
            title,
            0.0,
            "",
            0.0,
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            LenientBalanceCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBalanceCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBalanceCheckpointStatus next)
    {
        switch (Status)
        {
            case LenientBalanceCheckpointStatus.Proposed:
                return next == LenientBalanceCheckpointStatus.Approved || next == LenientBalanceCheckpointStatus.Withdrawn;
            case LenientBalanceCheckpointStatus.Approved:
                return next == LenientBalanceCheckpointStatus.Rejected || next == LenientBalanceCheckpointStatus.Withdrawn;
            case LenientBalanceCheckpointStatus.Rejected:
                switch (next)
                {
                    case LenientBalanceCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBalanceCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient balance checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient balance checkpoint"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount * Hours, 2);
    }

    public LenientBalanceCheckpoint Copy()
    {
        return new LenientBalanceCheckpoint(
            Code,
            Title,
            Hours,
            Currency,
            Ceiling,
            Discount,
            EmployeeId,
            Minimum,
            Country,
            ContactEmail,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
