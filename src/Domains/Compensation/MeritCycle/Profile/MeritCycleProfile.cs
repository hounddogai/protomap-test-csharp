namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle profile is in its life cycle.</summary>
public enum MeritCycleProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One merit cycle profile, with what the planners need to act on it.</summary>
public sealed class MeritCycleProfile(string code, string title, string reviewer, int expiresAt, double rate, double score, string region, string contactEmail, MeritCycleProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rate { get; set; } = rate;
    public double Score { get; set; } = score;
    public string Region { get; set; } = region;
    public string ContactEmail { get; set; } = contactEmail;
    public MeritCycleProfileStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle profile in status idle.</summary>
    public static MeritCycleProfile NewMeritCycleProfile(string code, string title)
    {
        return new MeritCycleProfile(code, title, "", 0, 0.0, 0.0, "", "", MeritCycleProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleProfileStatus next)
    {
        switch (Status)
        {
            case MeritCycleProfileStatus.Idle:
                return next == MeritCycleProfileStatus.Draining || next == MeritCycleProfileStatus.Stopped;
            case MeritCycleProfileStatus.Warming:
                return next == MeritCycleProfileStatus.Draining;
            case MeritCycleProfileStatus.Ready:
                return next == MeritCycleProfileStatus.Draining || next == MeritCycleProfileStatus.Stopped;
            case MeritCycleProfileStatus.Draining:
                return next == MeritCycleProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(MeritCycleProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"merit cycle profile"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Score + (double)ExpiresAt, 2);
    }

    public MeritCycleProfile Copy()
    {
        return new MeritCycleProfile(Code, Title, Reviewer, ExpiresAt, Rate, Score, Region, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
