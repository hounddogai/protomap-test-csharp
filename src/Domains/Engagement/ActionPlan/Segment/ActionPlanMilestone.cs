namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan milestone is in its life cycle.</summary>
public enum ActionPlanMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a action plan milestone and its current status.</summary>
public sealed class ActionPlanMilestone(string code, string label, int priority, string source, string badge, string team, string contactEmail, int endsAt, int version, string notes, int revision, ActionPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Source { get; set; } = source;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public int Revision { get; set; } = revision;
    public ActionPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a action plan milestone in status proposed.</summary>
    public static ActionPlanMilestone NewActionPlanMilestone(string code, string label)
    {
        return new ActionPlanMilestone(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            0,
            0,
            "",
            0,
            ActionPlanMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case ActionPlanMilestoneStatus.Proposed:
                return next == ActionPlanMilestoneStatus.Approved;
            case ActionPlanMilestoneStatus.Approved:
                switch (next)
                {
                    case ActionPlanMilestoneStatus.Rejected:
                    case ActionPlanMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case ActionPlanMilestoneStatus.Rejected:
                return next == ActionPlanMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ActionPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority * (double)EndsAt, 2);
    }

    public ActionPlanMilestone Copy()
    {
        return new ActionPlanMilestone(
            Code,
            Label,
            Priority,
            Source,
            Badge,
            Team,
            ContactEmail,
            EndsAt,
            Version,
            Notes,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
