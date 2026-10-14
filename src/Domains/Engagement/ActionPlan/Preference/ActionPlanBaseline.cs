namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan baseline is in its life cycle.</summary>
public enum ActionPlanBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One action plan baseline, with what the planners need to act on it.</summary>
public sealed class ActionPlanBaseline(string code, string label, string notes, int version, string summary, string category, int graceDays, string reviewer, string location, string phone, int sequence, ActionPlanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int Version { get; set; } = version;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public string Reviewer { get; set; } = reviewer;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public ActionPlanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a action plan baseline in status proposed.</summary>
    public static ActionPlanBaseline NewActionPlanBaseline(string code, string label)
    {
        return new ActionPlanBaseline(
            code,
            label,
            "",
            0,
            "",
            "",
            0,
            "",
            "",
            "",
            0,
            ActionPlanBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanBaselineStatus next)
    {
        switch (Status)
        {
            case ActionPlanBaselineStatus.Proposed:
                switch (next)
                {
                    case ActionPlanBaselineStatus.Rejected:
                    case ActionPlanBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case ActionPlanBaselineStatus.Approved:
                return next == ActionPlanBaselineStatus.Rejected || next == ActionPlanBaselineStatus.Withdrawn;
            case ActionPlanBaselineStatus.Rejected:
                switch (next)
                {
                    case ActionPlanBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version + (double)GraceDays, 2);
    }

    public ActionPlanBaseline Copy()
    {
        return new ActionPlanBaseline(
            Code,
            Label,
            Notes,
            Version,
            Summary,
            Category,
            GraceDays,
            Reviewer,
            Location,
            Phone,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
