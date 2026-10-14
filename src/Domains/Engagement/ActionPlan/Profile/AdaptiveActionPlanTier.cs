namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive action plan tier is in its life cycle.</summary>
public enum AdaptiveActionPlanTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive action plan tier, with what the planners need to act on it.</summary>
public sealed class AdaptiveActionPlanTier(string code, string name, string description, int limit, string ownerId, bool approved, double minimum, int createdAt, AdaptiveActionPlanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public string OwnerId { get; set; } = ownerId;
    public bool Approved { get; set; } = approved;
    public double Minimum { get; set; } = minimum;
    public int CreatedAt { get; set; } = createdAt;
    public AdaptiveActionPlanTierStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive action plan tier in status idle.</summary>
    public static AdaptiveActionPlanTier NewAdaptiveActionPlanTier(string code, string name)
    {
        return new AdaptiveActionPlanTier(code, name, "", 0, "", false, 0.0, 0, AdaptiveActionPlanTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveActionPlanTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveActionPlanTierStatus next)
    {
        switch (Status)
        {
            case AdaptiveActionPlanTierStatus.Idle:
                switch (next)
                {
                    case AdaptiveActionPlanTierStatus.Warming:
                    case AdaptiveActionPlanTierStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveActionPlanTierStatus.Warming:
                switch (next)
                {
                    case AdaptiveActionPlanTierStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveActionPlanTierStatus.Ready:
                return next == AdaptiveActionPlanTierStatus.Draining || next == AdaptiveActionPlanTierStatus.Stopped;
            case AdaptiveActionPlanTierStatus.Draining:
                switch (next)
                {
                    case AdaptiveActionPlanTierStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveActionPlanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive action plan tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)CreatedAt - Minimum, 2);
    }

    public AdaptiveActionPlanTier Copy()
    {
        return new AdaptiveActionPlanTier(
            Code,
            Name,
            Description,
            Limit,
            OwnerId,
            Approved,
            Minimum,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
