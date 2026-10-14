namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday estimate is in its life cycle.</summary>
public enum HolidayEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A holiday estimate. Its code is unique within a tenant.</summary>
public sealed class HolidayEstimate(string code, string title, string segment, double minimum, string salaryBand, bool visible, int limit, string jobCode, double floorValue, string reviewer, HolidayEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Minimum { get; set; } = minimum;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Visible { get; set; } = visible;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public double FloorValue { get; set; } = floorValue;
    public string Reviewer { get; set; } = reviewer;
    public HolidayEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a holiday estimate in status draft.</summary>
    public static HolidayEstimate NewHolidayEstimate(string code, string title)
    {
        return new HolidayEstimate(code, title, "", 0.0, "", false, 0, "", 0.0, "", HolidayEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayEstimateStatus next)
    {
        switch (Status)
        {
            case HolidayEstimateStatus.Draft:
                switch (next)
                {
                    case HolidayEstimateStatus.Suspended:
                    case HolidayEstimateStatus.Closed:
                        return true;
                }
                return false;
            case HolidayEstimateStatus.Pending:
                switch (next)
                {
                    case HolidayEstimateStatus.Suspended:
                        return true;
                }
                return false;
            case HolidayEstimateStatus.Active:
                return next == HolidayEstimateStatus.Suspended || next == HolidayEstimateStatus.Closed;
            case HolidayEstimateStatus.Suspended:
                return next == HolidayEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(HolidayEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round((double)Limit + FloorValue, 2);
    }

    public HolidayEstimate Copy()
    {
        return new HolidayEstimate(
            Code,
            Title,
            Segment,
            Minimum,
            SalaryBand,
            Visible,
            Limit,
            JobCode,
            FloorValue,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
