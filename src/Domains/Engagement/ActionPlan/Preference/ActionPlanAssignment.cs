namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan assignment is in its life cycle.</summary>
public enum ActionPlanAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A action plan assignment as the action plan team keeps it.</summary>
public sealed class ActionPlanAssignment(string code, string title, string segment, string location, string timezone, string notes, int revision, int expiresAt, string description, int endsAt, ActionPlanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public string Timezone { get; set; } = timezone;
    public string Notes { get; set; } = notes;
    public int Revision { get; set; } = revision;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Description { get; set; } = description;
    public int EndsAt { get; set; } = endsAt;
    public ActionPlanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a action plan assignment in status planned.</summary>
    public static ActionPlanAssignment NewActionPlanAssignment(string code, string title)
    {
        return new ActionPlanAssignment(
            code,
            title,
            "",
            "",
            "",
            "",
            0,
            0,
            "",
            0,
            ActionPlanAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanAssignmentStatus next)
    {
        switch (Status)
        {
            case ActionPlanAssignmentStatus.Planned:
                switch (next)
                {
                    case ActionPlanAssignmentStatus.Scheduled:
                    case ActionPlanAssignmentStatus.InProgress:
                        return true;
                }
                return false;
            case ActionPlanAssignmentStatus.Scheduled:
                return next == ActionPlanAssignmentStatus.InProgress;
            case ActionPlanAssignmentStatus.InProgress:
                switch (next)
                {
                    case ActionPlanAssignmentStatus.Completed:
                    case ActionPlanAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanAssignmentStatus.Completed:
                return next == ActionPlanAssignmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ActionPlanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round((double)EndsAt - (double)Revision, 2);
    }

    public ActionPlanAssignment Copy()
    {
        return new ActionPlanAssignment(
            Code,
            Title,
            Segment,
            Location,
            Timezone,
            Notes,
            Revision,
            ExpiresAt,
            Description,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
