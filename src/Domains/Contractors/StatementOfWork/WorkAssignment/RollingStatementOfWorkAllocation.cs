namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling statement of work allocation is in its life cycle.</summary>
public enum RollingStatementOfWorkAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling statement of work allocation, with what the planners need to act on it.</summary>
public sealed class RollingStatementOfWorkAllocation(string code, string title, double ratio, double discount, int revision, double ceiling, double minimum, RollingStatementOfWorkAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Discount { get; set; } = discount;
    public int Revision { get; set; } = revision;
    public double Ceiling { get; set; } = ceiling;
    public double Minimum { get; set; } = minimum;
    public RollingStatementOfWorkAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling statement of work allocation in status planned.</summary>
    public static RollingStatementOfWorkAllocation NewRollingStatementOfWorkAllocation(string code, string title)
    {
        return new RollingStatementOfWorkAllocation(
            code,
            title,
            0.0,
            0.0,
            0,
            0.0,
            0.0,
            RollingStatementOfWorkAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingStatementOfWorkAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingStatementOfWorkAllocationStatus next)
    {
        switch (Status)
        {
            case RollingStatementOfWorkAllocationStatus.Planned:
                switch (next)
                {
                    case RollingStatementOfWorkAllocationStatus.InProgress:
                    case RollingStatementOfWorkAllocationStatus.Completed:
                        return true;
                }
                return false;
            case RollingStatementOfWorkAllocationStatus.Scheduled:
                switch (next)
                {
                    case RollingStatementOfWorkAllocationStatus.Completed:
                    case RollingStatementOfWorkAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingStatementOfWorkAllocationStatus.InProgress:
                return next == RollingStatementOfWorkAllocationStatus.Completed || next == RollingStatementOfWorkAllocationStatus.Cancelled;
            case RollingStatementOfWorkAllocationStatus.Completed:
                switch (next)
                {
                    case RollingStatementOfWorkAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingStatementOfWorkAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling statement of work allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"rolling statement of work allocation"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Revision + Ceiling, 2);
    }

    public RollingStatementOfWorkAllocation Copy()
    {
        return new RollingStatementOfWorkAllocation(
            Code,
            Title,
            Ratio,
            Discount,
            Revision,
            Ceiling,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
