namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive position profile is in its life cycle.</summary>
public enum AdaptivePositionProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive position profile and its current status.</summary>
public sealed class AdaptivePositionProfile(string code, string title, double threshold, string summary, int priority, string description, int windowDays, string unit, bool active, string label, int startsAt, AdaptivePositionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public string Unit { get; set; } = unit;
    public bool Active { get; set; } = active;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public AdaptivePositionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive position profile in status requested.</summary>
    public static AdaptivePositionProfile NewAdaptivePositionProfile(string code, string title)
    {
        return new AdaptivePositionProfile(
            code,
            title,
            0.0,
            "",
            0,
            "",
            0,
            "",
            false,
            "",
            0,
            AdaptivePositionProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePositionProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePositionProfileStatus next)
    {
        switch (Status)
        {
            case AdaptivePositionProfileStatus.Requested:
                return next == AdaptivePositionProfileStatus.Granted;
            case AdaptivePositionProfileStatus.Granted:
                switch (next)
                {
                    case AdaptivePositionProfileStatus.Revoked:
                    case AdaptivePositionProfileStatus.Expired:
                        return true;
                }
                return false;
            case AdaptivePositionProfileStatus.Revoked:
                return next == AdaptivePositionProfileStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptivePositionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive position profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold - (double)WindowDays, 2);
    }

    public AdaptivePositionProfile Copy()
    {
        return new AdaptivePositionProfile(
            Code,
            Title,
            Threshold,
            Summary,
            Priority,
            Description,
            WindowDays,
            Unit,
            Active,
            Label,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
