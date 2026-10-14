namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual payslip estimate is in its life cycle.</summary>
public enum ManualPayslipEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual payslip estimate. Its code is unique within a tenant.</summary>
public sealed class ManualPayslipEstimate(string code, string title, double rate, string department, int tenureMonths, List<string> tags, ManualPayslipEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public List<string> Tags { get; set; } = tags;
    public ManualPayslipEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a manual payslip estimate in status draft.</summary>
    public static ManualPayslipEstimate NewManualPayslipEstimate(string code, string title)
    {
        return new ManualPayslipEstimate(
            code,
            title,
            0.0,
            "",
            0,
            new List<string>(),
            ManualPayslipEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPayslipEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPayslipEstimateStatus next)
    {
        switch (Status)
        {
            case ManualPayslipEstimateStatus.Draft:
                return next == ManualPayslipEstimateStatus.Pending || next == ManualPayslipEstimateStatus.Suspended;
            case ManualPayslipEstimateStatus.Pending:
                return next == ManualPayslipEstimateStatus.Active;
            case ManualPayslipEstimateStatus.Active:
                switch (next)
                {
                    case ManualPayslipEstimateStatus.Suspended:
                    case ManualPayslipEstimateStatus.Closed:
                        return true;
                }
                return false;
            case ManualPayslipEstimateStatus.Suspended:
                return next == ManualPayslipEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualPayslipEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual payslip estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double AdjustedLoad()
    {
        return Math.Round((double)TenureMonths * Rate, 2);
    }

    public ManualPayslipEstimate Copy()
    {
        return new ManualPayslipEstimate(Code, Title, Rate, Department, TenureMonths, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
