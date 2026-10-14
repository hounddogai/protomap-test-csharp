namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip notice is in its life cycle.</summary>
public enum PayslipNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A payslip notice as the payslip team keeps it.</summary>
public sealed class PayslipNotice(string code, string label, int priority, string description, int version, double score, PayslipNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public PayslipNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a payslip notice in status created.</summary>
    public static PayslipNotice NewPayslipNotice(string code, string label)
    {
        return new PayslipNotice(code, label, 0, "", 0, 0.0, PayslipNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipNoticeStatus next)
    {
        switch (Status)
        {
            case PayslipNoticeStatus.Created:
                switch (next)
                {
                    case PayslipNoticeStatus.Published:
                    case PayslipNoticeStatus.Retired:
                        return true;
                }
                return false;
            case PayslipNoticeStatus.Validated:
                switch (next)
                {
                    case PayslipNoticeStatus.Published:
                    case PayslipNoticeStatus.Retired:
                        return true;
                }
                return false;
            case PayslipNoticeStatus.Published:
                return next == PayslipNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayslipNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Priority + (double)Version, 2);
    }

    public PayslipNotice Copy()
    {
        return new PayslipNotice(Code, Label, Priority, Description, Version, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
