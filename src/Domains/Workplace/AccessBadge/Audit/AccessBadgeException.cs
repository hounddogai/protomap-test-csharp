namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge exception is in its life cycle.</summary>
public enum AccessBadgeExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A access badge exception as the access badge team keeps it.</summary>
public sealed class AccessBadgeException(string code, string title, string employeeId, int durationMinutes, double weight, AccessBadgeExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Weight { get; set; } = weight;
    public AccessBadgeExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a access badge exception in status draft.</summary>
    public static AccessBadgeException NewAccessBadgeException(string code, string title)
    {
        return new AccessBadgeException(code, title, "", 0, 0.0, AccessBadgeExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeExceptionStatus next)
    {
        switch (Status)
        {
            case AccessBadgeExceptionStatus.Draft:
                return next == AccessBadgeExceptionStatus.Suspended || next == AccessBadgeExceptionStatus.Closed;
            case AccessBadgeExceptionStatus.Pending:
                switch (next)
                {
                    case AccessBadgeExceptionStatus.Active:
                    case AccessBadgeExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case AccessBadgeExceptionStatus.Active:
                return next == AccessBadgeExceptionStatus.Closed;
            case AccessBadgeExceptionStatus.Suspended:
                return next == AccessBadgeExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"access badge exception"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)DurationMinutes + Weight, 2);
    }

    public AccessBadgeException Copy()
    {
        return new AccessBadgeException(Code, Title, EmployeeId, DurationMinutes, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
