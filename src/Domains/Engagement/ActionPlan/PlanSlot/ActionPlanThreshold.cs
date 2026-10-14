namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a action plan threshold is in its life cycle.</summary>
public enum ActionPlanThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One action plan threshold, with what the planners need to act on it.</summary>
public sealed class ActionPlanThreshold(string code, string label, string channel, int limit, double minimum, double fte, int endsAt, int revision, string jobCode, double rating, ActionPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public string JobCode { get; set; } = jobCode;
    public double Rating { get; set; } = rating;
    public ActionPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a action plan threshold in status idle.</summary>
    public static ActionPlanThreshold NewActionPlanThreshold(string code, string label)
    {
        return new ActionPlanThreshold(code, label, "", 0, 0.0, 0.0, 0, 0, "", 0.0, ActionPlanThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ActionPlanThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ActionPlanThresholdStatus next)
    {
        switch (Status)
        {
            case ActionPlanThresholdStatus.Idle:
                return next == ActionPlanThresholdStatus.Ready || next == ActionPlanThresholdStatus.Draining;
            case ActionPlanThresholdStatus.Warming:
                switch (next)
                {
                    case ActionPlanThresholdStatus.Ready:
                        return true;
                }
                return false;
            case ActionPlanThresholdStatus.Ready:
                switch (next)
                {
                    case ActionPlanThresholdStatus.Draining:
                        return true;
                }
                return false;
            case ActionPlanThresholdStatus.Draining:
                return next == ActionPlanThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ActionPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"action plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)EndsAt * Fte, 2);
    }

    public ActionPlanThreshold Copy()
    {
        return new ActionPlanThreshold(
            Code,
            Label,
            Channel,
            Limit,
            Minimum,
            Fte,
            EndsAt,
            Revision,
            JobCode,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
