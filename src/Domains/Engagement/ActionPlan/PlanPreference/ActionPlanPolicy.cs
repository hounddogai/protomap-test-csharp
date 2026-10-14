namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan policy is in its life cycle.</summary>
public enum ActionPlanPolicyStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a action plan policy and its current status.</summary>
public sealed class ActionPlanPolicy(string code, string name, double amount, int updatedAt, string department, string unit, int retries, ActionPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public int Retries { get; set; } = retries;
    public ActionPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a action plan policy in status draft.</summary>
    public static ActionPlanPolicy NewActionPlanPolicy(string code, string name)
    {
        return new ActionPlanPolicy(code, name, 0.0, 0, "", "", 0, ActionPlanPolicyStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanPolicyStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanPolicyStatus next)
    {
        switch (Status)
        {
            case ActionPlanPolicyStatus.Draft:
                switch (next)
                {
                    case ActionPlanPolicyStatus.Pending:
                    case ActionPlanPolicyStatus.Suspended:
                        return true;
                }
                return false;
            case ActionPlanPolicyStatus.Pending:
                switch (next)
                {
                    case ActionPlanPolicyStatus.Active:
                    case ActionPlanPolicyStatus.Closed:
                        return true;
                }
                return false;
            case ActionPlanPolicyStatus.Active:
                switch (next)
                {
                    case ActionPlanPolicyStatus.Suspended:
                        return true;
                }
                return false;
            case ActionPlanPolicyStatus.Suspended:
                return next == ActionPlanPolicyStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ActionPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Amount * (double)UpdatedAt, 2);
    }

    public ActionPlanPolicy Copy()
    {
        return new ActionPlanPolicy(Code, Name, Amount, UpdatedAt, Department, Unit, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
