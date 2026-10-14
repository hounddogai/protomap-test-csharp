namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan allocation is in its life cycle.</summary>
public enum ActionPlanAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A action plan allocation. Its code is unique within a tenant.</summary>
public sealed class ActionPlanAllocation(string code, string label, string channel, double amount, double hours, ActionPlanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public double Hours { get; set; } = hours;
    public ActionPlanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a action plan allocation in status created.</summary>
    public static ActionPlanAllocation NewActionPlanAllocation(string code, string label)
    {
        return new ActionPlanAllocation(code, label, "", 0.0, 0.0, ActionPlanAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanAllocationStatus next)
    {
        switch (Status)
        {
            case ActionPlanAllocationStatus.Created:
                return next == ActionPlanAllocationStatus.Validated || next == ActionPlanAllocationStatus.Published;
            case ActionPlanAllocationStatus.Validated:
                return next == ActionPlanAllocationStatus.Retired;
            case ActionPlanAllocationStatus.Published:
                switch (next)
                {
                    case ActionPlanAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetVolume()
    {
        return Math.Round(Amount * Hours, 2);
    }

    public ActionPlanAllocation Copy()
    {
        return new ActionPlanAllocation(Code, Label, Channel, Amount, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
