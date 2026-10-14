namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global payslip notice is in its life cycle.</summary>
public enum GlobalPayslipNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global payslip notice. Its code is unique within a tenant.</summary>
public sealed class GlobalPayslipNotice(string code, string name, string country, string unit, double maximum, double budget, string badge, double amount, string title, int quantity, GlobalPayslipNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public double Maximum { get; set; } = maximum;
    public double Budget { get; set; } = budget;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public GlobalPayslipNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a global payslip notice in status open.</summary>
    public static GlobalPayslipNotice NewGlobalPayslipNotice(string code, string name)
    {
        return new GlobalPayslipNotice(code, name, "", "", 0.0, 0.0, "", 0.0, "", 0, GlobalPayslipNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPayslipNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPayslipNoticeStatus next)
    {
        switch (Status)
        {
            case GlobalPayslipNoticeStatus.Open:
                switch (next)
                {
                    case GlobalPayslipNoticeStatus.Archived:
                        return true;
                }
                return false;
            case GlobalPayslipNoticeStatus.InReview:
                switch (next)
                {
                    case GlobalPayslipNoticeStatus.Resolved:
                        return true;
                }
                return false;
            case GlobalPayslipNoticeStatus.Resolved:
                switch (next)
                {
                    case GlobalPayslipNoticeStatus.Archived:
                        return true;
                }
                return false;
            case GlobalPayslipNoticeStatus.Reopened:
                switch (next)
                {
                    case GlobalPayslipNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPayslipNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global payslip notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Budget + Maximum, 2);
    }

    public GlobalPayslipNotice Copy()
    {
        return new GlobalPayslipNotice(
            Code,
            Name,
            Country,
            Unit,
            Maximum,
            Budget,
            Badge,
            Amount,
            Title,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
