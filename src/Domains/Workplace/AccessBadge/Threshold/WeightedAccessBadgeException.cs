namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted access badge exception is in its life cycle.</summary>
public enum WeightedAccessBadgeExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted access badge exception. Its code is unique within a tenant.</summary>
public sealed class WeightedAccessBadgeException(string code, string label, double fte, string employeeId, int updatedAt, string notes, double discount, string category, string salaryBand, double rate, string grade, WeightedAccessBadgeExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Notes { get; set; } = notes;
    public double Discount { get; set; } = discount;
    public string Category { get; set; } = category;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public WeightedAccessBadgeExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a weighted access badge exception in status proposed.</summary>
    public static WeightedAccessBadgeException NewWeightedAccessBadgeException(string code, string label)
    {
        return new WeightedAccessBadgeException(
            code,
            label,
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            WeightedAccessBadgeExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccessBadgeExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccessBadgeExceptionStatus next)
    {
        switch (Status)
        {
            case WeightedAccessBadgeExceptionStatus.Proposed:
                return next == WeightedAccessBadgeExceptionStatus.Withdrawn;
            case WeightedAccessBadgeExceptionStatus.Approved:
                return next == WeightedAccessBadgeExceptionStatus.Rejected || next == WeightedAccessBadgeExceptionStatus.Withdrawn;
            case WeightedAccessBadgeExceptionStatus.Rejected:
                return next == WeightedAccessBadgeExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedAccessBadgeExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted access badge exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Discount - Rate, 2);
    }

    public WeightedAccessBadgeException Copy()
    {
        return new WeightedAccessBadgeException(
            Code,
            Label,
            Fte,
            EmployeeId,
            UpdatedAt,
            Notes,
            Discount,
            Category,
            SalaryBand,
            Rate,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
