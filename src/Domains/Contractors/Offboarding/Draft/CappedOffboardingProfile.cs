namespace Acme.Workforce.Contractors.Offboarding.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped offboarding profile is in its life cycle.</summary>
public enum CappedOffboardingProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a capped offboarding profile and its current status.</summary>
public sealed class CappedOffboardingProfile(string code, string title, int endsAt, int expiresAt, int priority, double ceiling, string department, double rate, CappedOffboardingProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Priority { get; set; } = priority;
    public double Ceiling { get; set; } = ceiling;
    public string Department { get; set; } = department;
    public double Rate { get; set; } = rate;
    public CappedOffboardingProfileStatus Status { get; set; } = status;

    /// <summary>Starts a capped offboarding profile in status idle.</summary>
    public static CappedOffboardingProfile NewCappedOffboardingProfile(string code, string title)
    {
        return new CappedOffboardingProfile(code, title, 0, 0, 0, 0.0, "", 0.0, CappedOffboardingProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedOffboardingProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedOffboardingProfileStatus next)
    {
        switch (Status)
        {
            case CappedOffboardingProfileStatus.Idle:
                return next == CappedOffboardingProfileStatus.Ready;
            case CappedOffboardingProfileStatus.Warming:
                return next == CappedOffboardingProfileStatus.Draining || next == CappedOffboardingProfileStatus.Stopped;
            case CappedOffboardingProfileStatus.Ready:
                return next == CappedOffboardingProfileStatus.Draining || next == CappedOffboardingProfileStatus.Stopped;
            case CappedOffboardingProfileStatus.Draining:
                return next == CappedOffboardingProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedOffboardingProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped offboarding profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"capped offboarding profile"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Priority - Rate, 2);
    }

    public CappedOffboardingProfile Copy()
    {
        return new CappedOffboardingProfile(
            Code,
            Title,
            EndsAt,
            ExpiresAt,
            Priority,
            Ceiling,
            Department,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
