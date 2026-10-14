namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling accrual segment is in its life cycle.</summary>
public enum RollingAccrualSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One rolling accrual segment, with what the planners need to act on it.</summary>
public sealed class RollingAccrualSegment(string code, string title, double maximum, string locale, string location, double amount, string jobCode, string employeeId, string badge, RollingAccrualSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Locale { get; set; } = locale;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public string JobCode { get; set; } = jobCode;
    public string EmployeeId { get; set; } = employeeId;
    public string Badge { get; set; } = badge;
    public RollingAccrualSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling accrual segment in status proposed.</summary>
    public static RollingAccrualSegment NewRollingAccrualSegment(string code, string title)
    {
        return new RollingAccrualSegment(
            code,
            title,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            "",
            RollingAccrualSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccrualSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccrualSegmentStatus next)
    {
        switch (Status)
        {
            case RollingAccrualSegmentStatus.Proposed:
                switch (next)
                {
                    case RollingAccrualSegmentStatus.Approved:
                    case RollingAccrualSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case RollingAccrualSegmentStatus.Approved:
                return next == RollingAccrualSegmentStatus.Rejected;
            case RollingAccrualSegmentStatus.Rejected:
                switch (next)
                {
                    case RollingAccrualSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAccrualSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling accrual segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Amount - Maximum, 2);
    }

    public RollingAccrualSegment Copy()
    {
        return new RollingAccrualSegment(
            Code,
            Title,
            Maximum,
            Locale,
            Location,
            Amount,
            JobCode,
            EmployeeId,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
