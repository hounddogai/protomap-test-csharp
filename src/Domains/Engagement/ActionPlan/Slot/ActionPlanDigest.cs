namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan digest is in its life cycle.</summary>
public enum ActionPlanDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A action plan digest. Its code is unique within a tenant.</summary>
public sealed class ActionPlanDigest(string code, string title, int version, string channel, string unit, int sequence, double hours, ActionPlanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public string Unit { get; set; } = unit;
    public int Sequence { get; set; } = sequence;
    public double Hours { get; set; } = hours;
    public ActionPlanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a action plan digest in status open.</summary>
    public static ActionPlanDigest NewActionPlanDigest(string code, string title)
    {
        return new ActionPlanDigest(code, title, 0, "", "", 0, 0.0, ActionPlanDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanDigestStatus next)
    {
        switch (Status)
        {
            case ActionPlanDigestStatus.Open:
                return next == ActionPlanDigestStatus.Resolved || next == ActionPlanDigestStatus.Reopened;
            case ActionPlanDigestStatus.InReview:
                return next == ActionPlanDigestStatus.Reopened;
            case ActionPlanDigestStatus.Resolved:
                return next == ActionPlanDigestStatus.Reopened;
            case ActionPlanDigestStatus.Reopened:
                switch (next)
                {
                    case ActionPlanDigestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ActionPlanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"action plan digest"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Version * (double)Sequence, 2);
    }

    public ActionPlanDigest Copy()
    {
        return new ActionPlanDigest(Code, Title, Version, Channel, Unit, Sequence, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
