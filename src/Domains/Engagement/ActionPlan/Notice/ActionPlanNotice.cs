namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan notice is in its life cycle.</summary>
public enum ActionPlanNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One action plan notice, with what the planners need to act on it.</summary>
public sealed class ActionPlanNotice(string code, string name, List<string> tags, int durationMinutes, string badge, int sequence, double floorValue, ActionPlanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public double FloorValue { get; set; } = floorValue;
    public ActionPlanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a action plan notice in status proposed.</summary>
    public static ActionPlanNotice NewActionPlanNotice(string code, string name)
    {
        return new ActionPlanNotice(code, name, new List<string>(), 0, "", 0, 0.0, ActionPlanNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanNoticeStatus next)
    {
        switch (Status)
        {
            case ActionPlanNoticeStatus.Proposed:
                return next == ActionPlanNoticeStatus.Rejected || next == ActionPlanNoticeStatus.Withdrawn;
            case ActionPlanNoticeStatus.Approved:
                return next == ActionPlanNoticeStatus.Rejected || next == ActionPlanNoticeStatus.Withdrawn;
            case ActionPlanNoticeStatus.Rejected:
                switch (next)
                {
                    case ActionPlanNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedExposure()
    {
        return Math.Round(FloorValue * (double)Sequence, 2);
    }

    public ActionPlanNotice Copy()
    {
        return new ActionPlanNotice(Code, Name, Tags, DurationMinutes, Badge, Sequence, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
