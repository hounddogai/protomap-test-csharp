namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge cycle is in its life cycle.</summary>
public enum AccessBadgeCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A access badge cycle. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeCycle(string code, string title, double score, string employeeId, string currency, string summary, AccessBadgeCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public string Currency { get; set; } = currency;
    public string Summary { get; set; } = summary;
    public AccessBadgeCycleStatus Status { get; set; } = status;

    /// <summary>Starts a access badge cycle in status requested.</summary>
    public static AccessBadgeCycle NewAccessBadgeCycle(string code, string title)
    {
        return new AccessBadgeCycle(code, title, 0.0, "", "", "", AccessBadgeCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeCycleStatus next)
    {
        switch (Status)
        {
            case AccessBadgeCycleStatus.Requested:
                switch (next)
                {
                    case AccessBadgeCycleStatus.Granted:
                    case AccessBadgeCycleStatus.Expired:
                        return true;
                }
                return false;
            case AccessBadgeCycleStatus.Granted:
                return next == AccessBadgeCycleStatus.Revoked || next == AccessBadgeCycleStatus.Expired;
            case AccessBadgeCycleStatus.Revoked:
                switch (next)
                {
                    case AccessBadgeCycleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"access badge cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AccessBadgeCycle Copy()
    {
        return new AccessBadgeCycle(Code, Title, Score, EmployeeId, Currency, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
