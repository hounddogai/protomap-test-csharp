namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient deduction agreement is in its life cycle.</summary>
public enum LenientDeductionAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a lenient deduction agreement and its current status.</summary>
public sealed class LenientDeductionAgreement(string code, string title, string employeeId, bool visible, double amount, string locale, string region, string segment, double score, LenientDeductionAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public LenientDeductionAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a lenient deduction agreement in status proposed.</summary>
    public static LenientDeductionAgreement NewLenientDeductionAgreement(string code, string title)
    {
        return new LenientDeductionAgreement(
            code,
            title,
            "",
            false,
            0.0,
            "",
            "",
            "",
            0.0,
            LenientDeductionAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeductionAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeductionAgreementStatus next)
    {
        switch (Status)
        {
            case LenientDeductionAgreementStatus.Proposed:
                switch (next)
                {
                    case LenientDeductionAgreementStatus.Rejected:
                        return true;
                }
                return false;
            case LenientDeductionAgreementStatus.Approved:
                switch (next)
                {
                    case LenientDeductionAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientDeductionAgreementStatus.Rejected:
                return next == LenientDeductionAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientDeductionAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient deduction agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient deduction agreement"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Amount - Score, 2);
    }

    public LenientDeductionAgreement Copy()
    {
        return new LenientDeductionAgreement(
            Code,
            Title,
            EmployeeId,
            Visible,
            Amount,
            Locale,
            Region,
            Segment,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
