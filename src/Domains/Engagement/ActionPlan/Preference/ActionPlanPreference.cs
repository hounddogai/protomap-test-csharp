namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan preference is in its life cycle.</summary>
public enum ActionPlanPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One action plan preference, with what the planners need to act on it.</summary>
public sealed class ActionPlanPreference(string code, string name, string notes, double floorValue, double ratio, ActionPlanPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double FloorValue { get; set; } = floorValue;
    public double Ratio { get; set; } = ratio;
    public ActionPlanPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a action plan preference in status proposed.</summary>
    public static ActionPlanPreference NewActionPlanPreference(string code, string name)
    {
        return new ActionPlanPreference(code, name, "", 0.0, 0.0, ActionPlanPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanPreferenceStatus next)
    {
        switch (Status)
        {
            case ActionPlanPreferenceStatus.Proposed:
                return next == ActionPlanPreferenceStatus.Approved || next == ActionPlanPreferenceStatus.Rejected;
            case ActionPlanPreferenceStatus.Approved:
                return next == ActionPlanPreferenceStatus.Rejected || next == ActionPlanPreferenceStatus.Withdrawn;
            case ActionPlanPreferenceStatus.Rejected:
                switch (next)
                {
                    case ActionPlanPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue + Ratio, 2);
    }

    public ActionPlanPreference Copy()
    {
        return new ActionPlanPreference(Code, Name, Notes, FloorValue, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
