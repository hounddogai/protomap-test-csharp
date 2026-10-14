namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan budget is in its life cycle.</summary>
public enum ActionPlanBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One action plan budget, with what the planners need to act on it.</summary>
public sealed class ActionPlanBudget(string code, string title, Dictionary<string, string> attributes, string employeeId, int durationMinutes, string segment, double rate, string description, int startsAt, int graceDays, string department, ActionPlanBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public double Rate { get; set; } = rate;
    public string Description { get; set; } = description;
    public int StartsAt { get; set; } = startsAt;
    public int GraceDays { get; set; } = graceDays;
    public string Department { get; set; } = department;
    public ActionPlanBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a action plan budget in status new.</summary>
    public static ActionPlanBudget NewActionPlanBudget(string code, string title)
    {
        return new ActionPlanBudget(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            0.0,
            "",
            0,
            0,
            "",
            ActionPlanBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanBudgetStatus next)
    {
        switch (Status)
        {
            case ActionPlanBudgetStatus.New:
                switch (next)
                {
                    case ActionPlanBudgetStatus.Queued:
                    case ActionPlanBudgetStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanBudgetStatus.Queued:
                switch (next)
                {
                    case ActionPlanBudgetStatus.Running:
                    case ActionPlanBudgetStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanBudgetStatus.Running:
                return next == ActionPlanBudgetStatus.Done || next == ActionPlanBudgetStatus.Failed;
            case ActionPlanBudgetStatus.Done:
                switch (next)
                {
                    case ActionPlanBudgetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedTotal()
    {
        return Math.Round(Rate * (double)GraceDays, 2);
    }

    public ActionPlanBudget Copy()
    {
        return new ActionPlanBudget(
            Code,
            Title,
            Attributes,
            EmployeeId,
            DurationMinutes,
            Segment,
            Rate,
            Description,
            StartsAt,
            GraceDays,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
