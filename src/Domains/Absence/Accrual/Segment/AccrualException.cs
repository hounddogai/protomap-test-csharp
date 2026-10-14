namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual exception is in its life cycle.</summary>
public enum AccrualExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One accrual exception, with what the planners need to act on it.</summary>
public sealed class AccrualException(string code, string name, bool archived, string salaryBand, int attempts, int limit, double minimum, string managerId, int graceDays, double ceiling, double threshold, AccrualExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public double Ceiling { get; set; } = ceiling;
    public double Threshold { get; set; } = threshold;
    public AccrualExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a accrual exception in status planned.</summary>
    public static AccrualException NewAccrualException(string code, string name)
    {
        return new AccrualException(
            code,
            name,
            false,
            "",
            0,
            0,
            0.0,
            "",
            0,
            0.0,
            0.0,
            AccrualExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualExceptionStatus next)
    {
        switch (Status)
        {
            case AccrualExceptionStatus.Planned:
                return next == AccrualExceptionStatus.Completed;
            case AccrualExceptionStatus.Scheduled:
                switch (next)
                {
                    case AccrualExceptionStatus.InProgress:
                    case AccrualExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualExceptionStatus.InProgress:
                switch (next)
                {
                    case AccrualExceptionStatus.Completed:
                    case AccrualExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualExceptionStatus.Completed:
                return next == AccrualExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AccrualExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"accrual exception"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)GraceDays * Threshold, 2);
    }

    public AccrualException Copy()
    {
        return new AccrualException(
            Code,
            Name,
            Archived,
            SalaryBand,
            Attempts,
            Limit,
            Minimum,
            ManagerId,
            GraceDays,
            Ceiling,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
