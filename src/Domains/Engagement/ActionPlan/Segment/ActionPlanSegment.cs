namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan segment is in its life cycle.</summary>
public enum ActionPlanSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One action plan segment, with what the planners need to act on it.</summary>
public sealed class ActionPlanSegment(string code, string title, string channel, string timezone, double discount, ActionPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public double Discount { get; set; } = discount;
    public ActionPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a action plan segment in status planned.</summary>
    public static ActionPlanSegment NewActionPlanSegment(string code, string title)
    {
        return new ActionPlanSegment(code, title, "", "", 0.0, ActionPlanSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanSegmentStatus next)
    {
        switch (Status)
        {
            case ActionPlanSegmentStatus.Planned:
                switch (next)
                {
                    case ActionPlanSegmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanSegmentStatus.Scheduled:
                switch (next)
                {
                    case ActionPlanSegmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanSegmentStatus.InProgress:
                switch (next)
                {
                    case ActionPlanSegmentStatus.Cancelled:
                        return true;
                }
                return false;
            case ActionPlanSegmentStatus.Completed:
                return next == ActionPlanSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ActionPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"action plan segment"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public ActionPlanSegment Copy()
    {
        return new ActionPlanSegment(Code, Title, Channel, Timezone, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
