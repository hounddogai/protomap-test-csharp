namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback statement of work segment is in its life cycle.</summary>
public enum FallbackStatementOfWorkSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback statement of work segment. Its code is unique within a tenant.</summary>
public sealed class FallbackStatementOfWorkSegment(string code, string name, double cost, int limit, string phone, int graceDays, int capacity, string timezone, bool visible, string salaryBand, FallbackStatementOfWorkSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int Limit { get; set; } = limit;
    public string Phone { get; set; } = phone;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public string Timezone { get; set; } = timezone;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public FallbackStatementOfWorkSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback statement of work segment in status idle.</summary>
    public static FallbackStatementOfWorkSegment NewFallbackStatementOfWorkSegment(string code, string name)
    {
        return new FallbackStatementOfWorkSegment(
            code,
            name,
            0.0,
            0,
            "",
            0,
            0,
            "",
            false,
            "",
            FallbackStatementOfWorkSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackStatementOfWorkSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackStatementOfWorkSegmentStatus next)
    {
        switch (Status)
        {
            case FallbackStatementOfWorkSegmentStatus.Idle:
                switch (next)
                {
                    case FallbackStatementOfWorkSegmentStatus.Warming:
                        return true;
                }
                return false;
            case FallbackStatementOfWorkSegmentStatus.Warming:
                return next == FallbackStatementOfWorkSegmentStatus.Ready;
            case FallbackStatementOfWorkSegmentStatus.Ready:
                return next == FallbackStatementOfWorkSegmentStatus.Draining;
            case FallbackStatementOfWorkSegmentStatus.Draining:
                return next == FallbackStatementOfWorkSegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackStatementOfWorkSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback statement of work segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback statement of work segment"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)GraceDays + (double)Limit, 2);
    }

    public FallbackStatementOfWorkSegment Copy()
    {
        return new FallbackStatementOfWorkSegment(
            Code,
            Name,
            Cost,
            Limit,
            Phone,
            GraceDays,
            Capacity,
            Timezone,
            Visible,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
