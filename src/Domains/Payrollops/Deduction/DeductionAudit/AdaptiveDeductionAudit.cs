namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive deduction audit is in its life cycle.</summary>
public enum AdaptiveDeductionAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a adaptive deduction audit and its current status.</summary>
public sealed class AdaptiveDeductionAudit(string code, string label, int retries, string grade, bool active, AdaptiveDeductionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public AdaptiveDeductionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive deduction audit in status draft.</summary>
    public static AdaptiveDeductionAudit NewAdaptiveDeductionAudit(string code, string label)
    {
        return new AdaptiveDeductionAudit(code, label, 0, "", false, AdaptiveDeductionAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveDeductionAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveDeductionAuditStatus next)
    {
        switch (Status)
        {
            case AdaptiveDeductionAuditStatus.Draft:
                switch (next)
                {
                    case AdaptiveDeductionAuditStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptiveDeductionAuditStatus.Pending:
                return next == AdaptiveDeductionAuditStatus.Suspended || next == AdaptiveDeductionAuditStatus.Closed;
            case AdaptiveDeductionAuditStatus.Active:
                return next == AdaptiveDeductionAuditStatus.Suspended || next == AdaptiveDeductionAuditStatus.Closed;
            case AdaptiveDeductionAuditStatus.Suspended:
                return next == AdaptiveDeductionAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveDeductionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive deduction audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public AdaptiveDeductionAudit Copy()
    {
        return new AdaptiveDeductionAudit(Code, Label, Retries, Grade, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
