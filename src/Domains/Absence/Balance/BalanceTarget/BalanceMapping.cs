namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance mapping is in its life cycle.</summary>
public enum BalanceMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One balance mapping, with what the planners need to act on it.</summary>
public sealed class BalanceMapping(string code, string title, int priority, double rate, double floorValue, string location, int capacity, string summary, string employeeId, BalanceMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public double Rate { get; set; } = rate;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public int Capacity { get; set; } = capacity;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public BalanceMappingStatus Status { get; set; } = status;

    /// <summary>Starts a balance mapping in status open.</summary>
    public static BalanceMapping NewBalanceMapping(string code, string title)
    {
        return new BalanceMapping(code, title, 0, 0.0, 0.0, "", 0, "", "", BalanceMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceMappingStatus next)
    {
        switch (Status)
        {
            case BalanceMappingStatus.Open:
                return next == BalanceMappingStatus.InReview || next == BalanceMappingStatus.Reopened;
            case BalanceMappingStatus.InReview:
                switch (next)
                {
                    case BalanceMappingStatus.Resolved:
                        return true;
                }
                return false;
            case BalanceMappingStatus.Resolved:
                switch (next)
                {
                    case BalanceMappingStatus.Reopened:
                    case BalanceMappingStatus.Archived:
                        return true;
                }
                return false;
            case BalanceMappingStatus.Reopened:
                switch (next)
                {
                    case BalanceMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"balance mapping"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public BalanceMapping Copy()
    {
        return new BalanceMapping(
            Code,
            Title,
            Priority,
            Rate,
            FloorValue,
            Location,
            Capacity,
            Summary,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
