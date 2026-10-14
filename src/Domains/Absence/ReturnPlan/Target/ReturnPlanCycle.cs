namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan cycle is in its life cycle.</summary>
public enum ReturnPlanCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a return plan cycle and its current status.</summary>
public sealed class ReturnPlanCycle(string code, string title, string grade, int priority, string unit, double amount, string ownerId, bool active, ReturnPlanCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public string OwnerId { get; set; } = ownerId;
    public bool Active { get; set; } = active;
    public ReturnPlanCycleStatus Status { get; set; } = status;

    /// <summary>Starts a return plan cycle in status planned.</summary>
    public static ReturnPlanCycle NewReturnPlanCycle(string code, string title)
    {
        return new ReturnPlanCycle(code, title, "", 0, "", 0.0, "", false, ReturnPlanCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanCycleStatus next)
    {
        switch (Status)
        {
            case ReturnPlanCycleStatus.Planned:
                return next == ReturnPlanCycleStatus.Scheduled || next == ReturnPlanCycleStatus.Completed;
            case ReturnPlanCycleStatus.Scheduled:
                switch (next)
                {
                    case ReturnPlanCycleStatus.Completed:
                        return true;
                }
                return false;
            case ReturnPlanCycleStatus.InProgress:
                switch (next)
                {
                    case ReturnPlanCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case ReturnPlanCycleStatus.Completed:
                switch (next)
                {
                    case ReturnPlanCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round(Amount + (double)Priority, 2);
    }

    public ReturnPlanCycle Copy()
    {
        return new ReturnPlanCycle(Code, Title, Grade, Priority, Unit, Amount, OwnerId, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
