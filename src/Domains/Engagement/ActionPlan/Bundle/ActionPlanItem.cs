namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan item is in its life cycle.</summary>
public enum ActionPlanItemStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One action plan item, with what the planners need to act on it.</summary>
public sealed class ActionPlanItem(string code, string name, string managerId, double amount, string title, string employeeId, int leaveDays, string locale, string timezone, string notes, ActionPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public string Notes { get; set; } = notes;
    public ActionPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a action plan item in status requested.</summary>
    public static ActionPlanItem NewActionPlanItem(string code, string name)
    {
        return new ActionPlanItem(code, name, "", 0.0, "", "", 0, "", "", "", ActionPlanItemStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanItemStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanItemStatus next)
    {
        switch (Status)
        {
            case ActionPlanItemStatus.Requested:
                switch (next)
                {
                    case ActionPlanItemStatus.Revoked:
                        return true;
                }
                return false;
            case ActionPlanItemStatus.Granted:
                switch (next)
                {
                    case ActionPlanItemStatus.Revoked:
                    case ActionPlanItemStatus.Expired:
                        return true;
                }
                return false;
            case ActionPlanItemStatus.Revoked:
                switch (next)
                {
                    case ActionPlanItemStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Amount + (double)LeaveDays, 2);
    }

    public ActionPlanItem Copy()
    {
        return new ActionPlanItem(
            Code,
            Name,
            ManagerId,
            Amount,
            Title,
            EmployeeId,
            LeaveDays,
            Locale,
            Timezone,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
