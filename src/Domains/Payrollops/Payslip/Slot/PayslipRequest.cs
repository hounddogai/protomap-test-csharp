namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip request is in its life cycle.</summary>
public enum PayslipRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A payslip request. Its code is unique within a tenant.</summary>
public sealed class PayslipRequest(string code, string title, string reviewer, int retries, string category, int graceDays, double hours, string contactEmail, PayslipRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public PayslipRequestStatus Status { get; set; } = status;

    /// <summary>Starts a payslip request in status proposed.</summary>
    public static PayslipRequest NewPayslipRequest(string code, string title)
    {
        return new PayslipRequest(code, title, "", 0, "", 0, 0.0, "", PayslipRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipRequestStatus next)
    {
        switch (Status)
        {
            case PayslipRequestStatus.Proposed:
                return next == PayslipRequestStatus.Approved;
            case PayslipRequestStatus.Approved:
                return next == PayslipRequestStatus.Rejected || next == PayslipRequestStatus.Withdrawn;
            case PayslipRequestStatus.Rejected:
                return next == PayslipRequestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayslipRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)GraceDays - (double)Retries, 2);
    }

    public PayslipRequest Copy()
    {
        return new PayslipRequest(Code, Title, Reviewer, Retries, Category, GraceDays, Hours, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
