namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive action plan mapping is in its life cycle.</summary>
public enum AdaptiveActionPlanMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive action plan mapping, with what the planners need to act on it.</summary>
public sealed class AdaptiveActionPlanMapping(string code, string title, int expiresAt, string timezone, string checksum, double fte, string costCenter, int graceDays, AdaptiveActionPlanMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Timezone { get; set; } = timezone;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public int GraceDays { get; set; } = graceDays;
    public AdaptiveActionPlanMappingStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive action plan mapping in status open.</summary>
    public static AdaptiveActionPlanMapping NewAdaptiveActionPlanMapping(string code, string title)
    {
        return new AdaptiveActionPlanMapping(
            code,
            title,
            0,
            "",
            "",
            0.0,
            "",
            0,
            AdaptiveActionPlanMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveActionPlanMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveActionPlanMappingStatus next)
    {
        switch (Status)
        {
            case AdaptiveActionPlanMappingStatus.Open:
                switch (next)
                {
                    case AdaptiveActionPlanMappingStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveActionPlanMappingStatus.InReview:
                switch (next)
                {
                    case AdaptiveActionPlanMappingStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveActionPlanMappingStatus.Resolved:
                switch (next)
                {
                    case AdaptiveActionPlanMappingStatus.Reopened:
                    case AdaptiveActionPlanMappingStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveActionPlanMappingStatus.Reopened:
                return next == AdaptiveActionPlanMappingStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveActionPlanMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive action plan mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive action plan mapping"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)ExpiresAt - Fte, 2);
    }

    public AdaptiveActionPlanMapping Copy()
    {
        return new AdaptiveActionPlanMapping(
            Code,
            Title,
            ExpiresAt,
            Timezone,
            Checksum,
            Fte,
            CostCenter,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
