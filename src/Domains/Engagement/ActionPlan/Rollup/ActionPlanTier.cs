namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan tier is in its life cycle.</summary>
public enum ActionPlanTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A action plan tier. Its code is unique within a tenant.</summary>
public sealed class ActionPlanTier(string code, string label, int retries, int tenureMonths, string jobCode, double floorValue, ActionPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int TenureMonths { get; set; } = tenureMonths;
    public string JobCode { get; set; } = jobCode;
    public double FloorValue { get; set; } = floorValue;
    public ActionPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a action plan tier in status created.</summary>
    public static ActionPlanTier NewActionPlanTier(string code, string label)
    {
        return new ActionPlanTier(code, label, 0, 0, "", 0.0, ActionPlanTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanTierStatus next)
    {
        switch (Status)
        {
            case ActionPlanTierStatus.Created:
                return next == ActionPlanTierStatus.Published || next == ActionPlanTierStatus.Retired;
            case ActionPlanTierStatus.Validated:
                switch (next)
                {
                    case ActionPlanTierStatus.Retired:
                        return true;
                }
                return false;
            case ActionPlanTierStatus.Published:
                switch (next)
                {
                    case ActionPlanTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"action plan tier"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries - (double)TenureMonths, 2);
    }

    public ActionPlanTier Copy()
    {
        return new ActionPlanTier(Code, Label, Retries, TenureMonths, JobCode, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
