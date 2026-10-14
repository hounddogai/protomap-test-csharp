namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped payslip rule is in its life cycle.</summary>
public enum CappedPayslipRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped payslip rule, with what the planners need to act on it.</summary>
public sealed class CappedPayslipRule(string code, string title, double discount, List<string> tags, string salaryBand, int quantity, string name, bool active, string checksum, bool visible, CappedPayslipRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public List<string> Tags { get; set; } = tags;
    public string SalaryBand { get; set; } = salaryBand;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public CappedPayslipRuleStatus Status { get; set; } = status;

    /// <summary>Starts a capped payslip rule in status draft.</summary>
    public static CappedPayslipRule NewCappedPayslipRule(string code, string title)
    {
        return new CappedPayslipRule(
            code,
            title,
            0.0,
            new List<string>(),
            "",
            0,
            "",
            false,
            "",
            false,
            CappedPayslipRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayslipRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayslipRuleStatus next)
    {
        switch (Status)
        {
            case CappedPayslipRuleStatus.Draft:
                switch (next)
                {
                    case CappedPayslipRuleStatus.Active:
                    case CappedPayslipRuleStatus.Suspended:
                        return true;
                }
                return false;
            case CappedPayslipRuleStatus.Pending:
                return next == CappedPayslipRuleStatus.Closed;
            case CappedPayslipRuleStatus.Active:
                switch (next)
                {
                    case CappedPayslipRuleStatus.Closed:
                        return true;
                }
                return false;
            case CappedPayslipRuleStatus.Suspended:
                return next == CappedPayslipRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedPayslipRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped payslip rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Quantity + Discount, 2);
    }

    public CappedPayslipRule Copy()
    {
        return new CappedPayslipRule(
            Code,
            Title,
            Discount,
            Tags,
            SalaryBand,
            Quantity,
            Name,
            Active,
            Checksum,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
