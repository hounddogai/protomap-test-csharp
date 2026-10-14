namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate segment is in its life cycle.</summary>
public enum RateSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rate segment as the rate team keeps it.</summary>
public sealed class RateSegment(string code, string label, string jobCode, string contactEmail, int tenureMonths, string currency, string employeeId, string salaryBand, RateSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Currency { get; set; } = currency;
    public string EmployeeId { get; set; } = employeeId;
    public string SalaryBand { get; set; } = salaryBand;
    public RateSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a rate segment in status proposed.</summary>
    public static RateSegment NewRateSegment(string code, string label)
    {
        return new RateSegment(code, label, "", "", 0, "", "", "", RateSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateSegmentStatus next)
    {
        switch (Status)
        {
            case RateSegmentStatus.Proposed:
                return next == RateSegmentStatus.Rejected || next == RateSegmentStatus.Withdrawn;
            case RateSegmentStatus.Approved:
                return next == RateSegmentStatus.Rejected || next == RateSegmentStatus.Withdrawn;
            case RateSegmentStatus.Rejected:
                switch (next)
                {
                    case RateSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public RateSegment Copy()
    {
        return new RateSegment(
            Code,
            Label,
            JobCode,
            ContactEmail,
            TenureMonths,
            Currency,
            EmployeeId,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
