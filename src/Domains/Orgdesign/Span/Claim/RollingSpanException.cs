namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling span exception is in its life cycle.</summary>
public enum RollingSpanExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling span exception, with what the planners need to act on it.</summary>
public sealed class RollingSpanException(string code, string title, int leaveDays, string employeeId, string name, double amount, RollingSpanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string EmployeeId { get; set; } = employeeId;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public RollingSpanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a rolling span exception in status planned.</summary>
    public static RollingSpanException NewRollingSpanException(string code, string title)
    {
        return new RollingSpanException(code, title, 0, "", "", 0.0, RollingSpanExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSpanExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSpanExceptionStatus next)
    {
        switch (Status)
        {
            case RollingSpanExceptionStatus.Planned:
                switch (next)
                {
                    case RollingSpanExceptionStatus.Completed:
                    case RollingSpanExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingSpanExceptionStatus.Scheduled:
                return next == RollingSpanExceptionStatus.Cancelled;
            case RollingSpanExceptionStatus.InProgress:
                switch (next)
                {
                    case RollingSpanExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingSpanExceptionStatus.Completed:
                switch (next)
                {
                    case RollingSpanExceptionStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSpanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling span exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Amount - (double)LeaveDays, 2);
    }

    public RollingSpanException Copy()
    {
        return new RollingSpanException(Code, Title, LeaveDays, EmployeeId, Name, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
