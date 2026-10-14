namespace Acme.Workforce.Payrollops.Deduction.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction agreement is in its life cycle.</summary>
public enum DeductionAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A deduction agreement. Its code is unique within a tenant.</summary>
public sealed class DeductionAgreement(string code, string label, bool approved, string source, bool pinned, bool active, int windowDays, double ceiling, DeductionAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public int WindowDays { get; set; } = windowDays;
    public double Ceiling { get; set; } = ceiling;
    public DeductionAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a deduction agreement in status proposed.</summary>
    public static DeductionAgreement NewDeductionAgreement(string code, string label)
    {
        return new DeductionAgreement(
            code,
            label,
            false,
            "",
            false,
            false,
            0,
            0.0,
            DeductionAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionAgreementStatus next)
    {
        switch (Status)
        {
            case DeductionAgreementStatus.Proposed:
                switch (next)
                {
                    case DeductionAgreementStatus.Approved:
                        return true;
                }
                return false;
            case DeductionAgreementStatus.Approved:
                switch (next)
                {
                    case DeductionAgreementStatus.Rejected:
                    case DeductionAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeductionAgreementStatus.Rejected:
                return next == DeductionAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeductionAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)WindowDays + Ceiling, 2);
    }

    public DeductionAgreement Copy()
    {
        return new DeductionAgreement(Code, Label, Approved, Source, Pinned, Active, WindowDays, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
