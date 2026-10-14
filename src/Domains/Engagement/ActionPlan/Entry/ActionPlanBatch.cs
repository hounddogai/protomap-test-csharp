namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan batch is in its life cycle.</summary>
public enum ActionPlanBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a action plan batch and its current status.</summary>
public sealed class ActionPlanBatch(string code, string label, string department, bool active, string skill, int createdAt, ActionPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public int CreatedAt { get; set; } = createdAt;
    public ActionPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a action plan batch in status created.</summary>
    public static ActionPlanBatch NewActionPlanBatch(string code, string label)
    {
        return new ActionPlanBatch(code, label, "", false, "", 0, ActionPlanBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanBatchStatus next)
    {
        switch (Status)
        {
            case ActionPlanBatchStatus.Created:
                return next == ActionPlanBatchStatus.Validated || next == ActionPlanBatchStatus.Published;
            case ActionPlanBatchStatus.Validated:
                return next == ActionPlanBatchStatus.Retired;
            case ActionPlanBatchStatus.Published:
                switch (next)
                {
                    case ActionPlanBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public ActionPlanBatch Copy()
    {
        return new ActionPlanBatch(Code, Label, Department, Active, Skill, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
