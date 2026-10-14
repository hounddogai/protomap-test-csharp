namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay rule is in its life cycle.</summary>
public enum RetroPayRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One retro pay rule, with what the planners need to act on it.</summary>
public sealed class RetroPayRule(string code, string title, string channel, double budget, string employeeId, RetroPayRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public string EmployeeId { get; set; } = employeeId;
    public RetroPayRuleStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay rule in status planned.</summary>
    public static RetroPayRule NewRetroPayRule(string code, string title)
    {
        return new RetroPayRule(code, title, "", 0.0, "", RetroPayRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayRuleStatus next)
    {
        switch (Status)
        {
            case RetroPayRuleStatus.Planned:
                switch (next)
                {
                    case RetroPayRuleStatus.Completed:
                    case RetroPayRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayRuleStatus.Scheduled:
                return next == RetroPayRuleStatus.InProgress;
            case RetroPayRuleStatus.InProgress:
                return next == RetroPayRuleStatus.Cancelled;
            case RetroPayRuleStatus.Completed:
                switch (next)
                {
                    case RetroPayRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public RetroPayRule Copy()
    {
        return new RetroPayRule(Code, Title, Channel, Budget, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
