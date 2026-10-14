namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan schedule is in its life cycle.</summary>
public enum ActionPlanScheduleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One action plan schedule, with what the planners need to act on it.</summary>
public sealed class ActionPlanSchedule(string code, string name, double score, string unit, int leaveDays, int retries, ActionPlanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public string Unit { get; set; } = unit;
    public int LeaveDays { get; set; } = leaveDays;
    public int Retries { get; set; } = retries;
    public ActionPlanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a action plan schedule in status draft.</summary>
    public static ActionPlanSchedule NewActionPlanSchedule(string code, string name)
    {
        return new ActionPlanSchedule(code, name, 0.0, "", 0, 0, ActionPlanScheduleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanScheduleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanScheduleStatus next)
    {
        switch (Status)
        {
            case ActionPlanScheduleStatus.Draft:
                return next == ActionPlanScheduleStatus.Active || next == ActionPlanScheduleStatus.Closed;
            case ActionPlanScheduleStatus.Pending:
                switch (next)
                {
                    case ActionPlanScheduleStatus.Suspended:
                    case ActionPlanScheduleStatus.Closed:
                        return true;
                }
                return false;
            case ActionPlanScheduleStatus.Active:
                return next == ActionPlanScheduleStatus.Closed;
            case ActionPlanScheduleStatus.Suspended:
                return next == ActionPlanScheduleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ActionPlanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round((double)LeaveDays - Score, 2);
    }

    public ActionPlanSchedule Copy()
    {
        return new ActionPlanSchedule(Code, Name, Score, Unit, LeaveDays, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
