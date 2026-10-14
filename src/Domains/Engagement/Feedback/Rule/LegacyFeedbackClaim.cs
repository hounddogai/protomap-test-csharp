namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy feedback claim is in its life cycle.</summary>
public enum LegacyFeedbackClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy feedback claim as the feedback team keeps it.</summary>
public sealed class LegacyFeedbackClaim(string code, string label, int endsAt, string location, string country, string source, int createdAt, string description, string externalRef, double cost, LegacyFeedbackClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Location { get; set; } = location;
    public string Country { get; set; } = country;
    public string Source { get; set; } = source;
    public int CreatedAt { get; set; } = createdAt;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public double Cost { get; set; } = cost;
    public LegacyFeedbackClaimStatus Status { get; set; } = status;

    /// <summary>Starts a legacy feedback claim in status idle.</summary>
    public static LegacyFeedbackClaim NewLegacyFeedbackClaim(string code, string label)
    {
        return new LegacyFeedbackClaim(code, label, 0, "", "", "", 0, "", "", 0.0, LegacyFeedbackClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyFeedbackClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyFeedbackClaimStatus next)
    {
        switch (Status)
        {
            case LegacyFeedbackClaimStatus.Idle:
                switch (next)
                {
                    case LegacyFeedbackClaimStatus.Warming:
                        return true;
                }
                return false;
            case LegacyFeedbackClaimStatus.Warming:
                return next == LegacyFeedbackClaimStatus.Draining;
            case LegacyFeedbackClaimStatus.Ready:
                return next == LegacyFeedbackClaimStatus.Draining || next == LegacyFeedbackClaimStatus.Stopped;
            case LegacyFeedbackClaimStatus.Draining:
                switch (next)
                {
                    case LegacyFeedbackClaimStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyFeedbackClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy feedback claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"legacy feedback claim"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Cost - (double)CreatedAt, 2);
    }

    public LegacyFeedbackClaim Copy()
    {
        return new LegacyFeedbackClaim(
            Code,
            Label,
            EndsAt,
            Location,
            Country,
            Source,
            CreatedAt,
            Description,
            ExternalRef,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
