namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive action plan milestone is in its life cycle.</summary>
public enum AdaptiveActionPlanMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive action plan milestone. Its code is unique within a tenant.</summary>
public sealed class AdaptiveActionPlanMilestone(string code, string title, bool archived, int startsAt, string source, int durationMinutes, AdaptiveActionPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int StartsAt { get; set; } = startsAt;
    public string Source { get; set; } = source;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AdaptiveActionPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive action plan milestone in status idle.</summary>
    public static AdaptiveActionPlanMilestone NewAdaptiveActionPlanMilestone(string code, string title)
    {
        return new AdaptiveActionPlanMilestone(code, title, false, 0, "", 0, AdaptiveActionPlanMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveActionPlanMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveActionPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case AdaptiveActionPlanMilestoneStatus.Idle:
                switch (next)
                {
                    case AdaptiveActionPlanMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveActionPlanMilestoneStatus.Warming:
                switch (next)
                {
                    case AdaptiveActionPlanMilestoneStatus.Ready:
                    case AdaptiveActionPlanMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveActionPlanMilestoneStatus.Ready:
                return next == AdaptiveActionPlanMilestoneStatus.Stopped;
            case AdaptiveActionPlanMilestoneStatus.Draining:
                switch (next)
                {
                    case AdaptiveActionPlanMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveActionPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive action plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)StartsAt - (double)DurationMinutes, 2);
    }

    public AdaptiveActionPlanMilestone Copy()
    {
        return new AdaptiveActionPlanMilestone(Code, Title, Archived, StartsAt, Source, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
