namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled payslip template is in its life cycle.</summary>
public enum ScheduledPayslipTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled payslip template. Its code is unique within a tenant.</summary>
public sealed class ScheduledPayslipTemplate(string code, string label, string name, int leaveDays, int graceDays, string currency, ScheduledPayslipTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int GraceDays { get; set; } = graceDays;
    public string Currency { get; set; } = currency;
    public ScheduledPayslipTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled payslip template in status open.</summary>
    public static ScheduledPayslipTemplate NewScheduledPayslipTemplate(string code, string label)
    {
        return new ScheduledPayslipTemplate(code, label, "", 0, 0, "", ScheduledPayslipTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayslipTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayslipTemplateStatus next)
    {
        switch (Status)
        {
            case ScheduledPayslipTemplateStatus.Open:
                return next == ScheduledPayslipTemplateStatus.InReview || next == ScheduledPayslipTemplateStatus.Resolved;
            case ScheduledPayslipTemplateStatus.InReview:
                switch (next)
                {
                    case ScheduledPayslipTemplateStatus.Reopened:
                    case ScheduledPayslipTemplateStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledPayslipTemplateStatus.Resolved:
                return next == ScheduledPayslipTemplateStatus.Reopened;
            case ScheduledPayslipTemplateStatus.Reopened:
                return next == ScheduledPayslipTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledPayslipTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled payslip template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)LeaveDays - (double)GraceDays, 2);
    }

    public ScheduledPayslipTemplate Copy()
    {
        return new ScheduledPayslipTemplate(Code, Label, Name, LeaveDays, GraceDays, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
