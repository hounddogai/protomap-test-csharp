namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate cycle is in its life cycle.</summary>
public enum RateCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rate cycle. Its code is unique within a tenant.</summary>
public sealed class RateCycle(string code, string title, string salaryBand, string employeeId, int startsAt, RateCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public RateCycleStatus Status { get; set; } = status;

    /// <summary>Starts a rate cycle in status proposed.</summary>
    public static RateCycle NewRateCycle(string code, string title)
    {
        return new RateCycle(code, title, "", "", 0, RateCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateCycleStatus next)
    {
        switch (Status)
        {
            case RateCycleStatus.Proposed:
                switch (next)
                {
                    case RateCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case RateCycleStatus.Approved:
                switch (next)
                {
                    case RateCycleStatus.Withdrawn:
                        return true;
                }
                return false;
            case RateCycleStatus.Rejected:
                return next == RateCycleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RateCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public RateCycle Copy()
    {
        return new RateCycle(Code, Title, SalaryBand, EmployeeId, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
