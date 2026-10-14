namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan profile is in its life cycle.</summary>
public enum ActionPlanProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One action plan profile, with what the planners need to act on it.</summary>
public sealed class ActionPlanProfile(string code, string title, string employeeId, string managerId, string channel, string timezone, double amount, ActionPlanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public double Amount { get; set; } = amount;
    public ActionPlanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a action plan profile in status open.</summary>
    public static ActionPlanProfile NewActionPlanProfile(string code, string title)
    {
        return new ActionPlanProfile(code, title, "", "", "", "", 0.0, ActionPlanProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanProfileStatus next)
    {
        switch (Status)
        {
            case ActionPlanProfileStatus.Open:
                switch (next)
                {
                    case ActionPlanProfileStatus.InReview:
                    case ActionPlanProfileStatus.Archived:
                        return true;
                }
                return false;
            case ActionPlanProfileStatus.InReview:
                return next == ActionPlanProfileStatus.Resolved || next == ActionPlanProfileStatus.Archived;
            case ActionPlanProfileStatus.Resolved:
                return next == ActionPlanProfileStatus.Archived;
            case ActionPlanProfileStatus.Reopened:
                switch (next)
                {
                    case ActionPlanProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ActionPlanProfile Copy()
    {
        return new ActionPlanProfile(Code, Title, EmployeeId, ManagerId, Channel, Timezone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
