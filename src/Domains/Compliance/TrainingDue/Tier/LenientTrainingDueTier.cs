namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient training due tier is in its life cycle.</summary>
public enum LenientTrainingDueTierStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One lenient training due tier, with what the planners need to act on it.</summary>
public sealed class LenientTrainingDueTier(string code, string title, string location, string channel, string summary, int expiresAt, string ownerId, double ceiling, double threshold, LenientTrainingDueTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Channel { get; set; } = channel;
    public string Summary { get; set; } = summary;
    public int ExpiresAt { get; set; } = expiresAt;
    public string OwnerId { get; set; } = ownerId;
    public double Ceiling { get; set; } = ceiling;
    public double Threshold { get; set; } = threshold;
    public LenientTrainingDueTierStatus Status { get; set; } = status;

    /// <summary>Starts a lenient training due tier in status idle.</summary>
    public static LenientTrainingDueTier NewLenientTrainingDueTier(string code, string title)
    {
        return new LenientTrainingDueTier(
            code,
            title,
            "",
            "",
            "",
            0,
            "",
            0.0,
            0.0,
            LenientTrainingDueTierStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTrainingDueTierStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTrainingDueTierStatus next)
    {
        switch (Status)
        {
            case LenientTrainingDueTierStatus.Idle:
                return next == LenientTrainingDueTierStatus.Warming || next == LenientTrainingDueTierStatus.Stopped;
            case LenientTrainingDueTierStatus.Warming:
                return next == LenientTrainingDueTierStatus.Stopped;
            case LenientTrainingDueTierStatus.Ready:
                switch (next)
                {
                    case LenientTrainingDueTierStatus.Draining:
                        return true;
                }
                return false;
            case LenientTrainingDueTierStatus.Draining:
                return next == LenientTrainingDueTierStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientTrainingDueTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient training due tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ceiling - (double)ExpiresAt, 2);
    }

    public LenientTrainingDueTier Copy()
    {
        return new LenientTrainingDueTier(
            Code,
            Title,
            Location,
            Channel,
            Summary,
            ExpiresAt,
            OwnerId,
            Ceiling,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
