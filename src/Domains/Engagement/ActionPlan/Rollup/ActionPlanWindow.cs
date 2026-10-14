namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan window is in its life cycle.</summary>
public enum ActionPlanWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A action plan window. Its code is unique within a tenant.</summary>
public sealed class ActionPlanWindow(string code, string title, int expiresAt, string timezone, int createdAt, double amount, ActionPlanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Timezone { get; set; } = timezone;
    public int CreatedAt { get; set; } = createdAt;
    public double Amount { get; set; } = amount;
    public ActionPlanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a action plan window in status open.</summary>
    public static ActionPlanWindow NewActionPlanWindow(string code, string title)
    {
        return new ActionPlanWindow(code, title, 0, "", 0, 0.0, ActionPlanWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanWindowStatus next)
    {
        switch (Status)
        {
            case ActionPlanWindowStatus.Open:
                return next == ActionPlanWindowStatus.Resolved;
            case ActionPlanWindowStatus.InReview:
                switch (next)
                {
                    case ActionPlanWindowStatus.Resolved:
                    case ActionPlanWindowStatus.Archived:
                        return true;
                }
                return false;
            case ActionPlanWindowStatus.Resolved:
                switch (next)
                {
                    case ActionPlanWindowStatus.Reopened:
                    case ActionPlanWindowStatus.Archived:
                        return true;
                }
                return false;
            case ActionPlanWindowStatus.Reopened:
                switch (next)
                {
                    case ActionPlanWindowStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)CreatedAt - Amount, 2);
    }

    public ActionPlanWindow Copy()
    {
        return new ActionPlanWindow(Code, Title, ExpiresAt, Timezone, CreatedAt, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
