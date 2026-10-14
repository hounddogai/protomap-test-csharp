namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction audit is in its life cycle.</summary>
public enum DeductionAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One deduction audit, with what the planners need to act on it.</summary>
public sealed class DeductionAudit(string code, string name, string region, double margin, string department, string currency, int leaveDays, double rate, DeductionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public double Margin { get; set; } = margin;
    public string Department { get; set; } = department;
    public string Currency { get; set; } = currency;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public DeductionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a deduction audit in status created.</summary>
    public static DeductionAudit NewDeductionAudit(string code, string name)
    {
        return new DeductionAudit(code, name, "", 0.0, "", "", 0, 0.0, DeductionAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionAuditStatus next)
    {
        switch (Status)
        {
            case DeductionAuditStatus.Created:
                return next == DeductionAuditStatus.Published || next == DeductionAuditStatus.Retired;
            case DeductionAuditStatus.Validated:
                return next == DeductionAuditStatus.Retired;
            case DeductionAuditStatus.Published:
                switch (next)
                {
                    case DeductionAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round(Rate - (double)LeaveDays, 2);
    }

    public DeductionAudit Copy()
    {
        return new DeductionAudit(Code, Name, Region, Margin, Department, Currency, LeaveDays, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
