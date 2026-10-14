namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay run cycle is in its life cycle.</summary>
public enum DefaultPayRunCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default pay run cycle, with what the planners need to act on it.</summary>
public sealed class DefaultPayRunCycle(string code, string title, string jobCode, double rate, string ownerId, DefaultPayRunCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public double Rate { get; set; } = rate;
    public string OwnerId { get; set; } = ownerId;
    public DefaultPayRunCycleStatus Status { get; set; } = status;

    /// <summary>Starts a default pay run cycle in status proposed.</summary>
    public static DefaultPayRunCycle NewDefaultPayRunCycle(string code, string title)
    {
        return new DefaultPayRunCycle(code, title, "", 0.0, "", DefaultPayRunCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayRunCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayRunCycleStatus next)
    {
        switch (Status)
        {
            case DefaultPayRunCycleStatus.Proposed:
                switch (next)
                {
                    case DefaultPayRunCycleStatus.Approved:
                    case DefaultPayRunCycleStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultPayRunCycleStatus.Approved:
                return next == DefaultPayRunCycleStatus.Rejected || next == DefaultPayRunCycleStatus.Withdrawn;
            case DefaultPayRunCycleStatus.Rejected:
                switch (next)
                {
                    case DefaultPayRunCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayRunCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay run cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public DefaultPayRunCycle Copy()
    {
        return new DefaultPayRunCycle(Code, Title, JobCode, Rate, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
