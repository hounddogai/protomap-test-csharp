namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan variance is in its life cycle.</summary>
public enum ActionPlanVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a action plan variance and its current status.</summary>
public sealed class ActionPlanVariance(string code, string label, string name, string ownerId, string employeeId, int createdAt, int capacity, ActionPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public ActionPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a action plan variance in status new.</summary>
    public static ActionPlanVariance NewActionPlanVariance(string code, string label)
    {
        return new ActionPlanVariance(code, label, "", "", "", 0, 0, ActionPlanVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanVarianceStatus next)
    {
        switch (Status)
        {
            case ActionPlanVarianceStatus.New:
                switch (next)
                {
                    case ActionPlanVarianceStatus.Running:
                    case ActionPlanVarianceStatus.Failed:
                        return true;
                }
                return false;
            case ActionPlanVarianceStatus.Queued:
                return next == ActionPlanVarianceStatus.Done || next == ActionPlanVarianceStatus.Failed;
            case ActionPlanVarianceStatus.Running:
                switch (next)
                {
                    case ActionPlanVarianceStatus.Done:
                        return true;
                }
                return false;
            case ActionPlanVarianceStatus.Done:
                return next == ActionPlanVarianceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ActionPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity * (double)CreatedAt, 2);
    }

    public ActionPlanVariance Copy()
    {
        return new ActionPlanVariance(Code, Label, Name, OwnerId, EmployeeId, CreatedAt, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
