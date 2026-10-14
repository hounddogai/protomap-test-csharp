namespace Acme.Workforce.Talent.Mobility.MobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped mobility override is in its life cycle.</summary>
public enum CappedMobilityOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a capped mobility override and its current status.</summary>
public sealed class CappedMobilityOverride(string code, string title, string skill, string team, int attempts, double cost, string region, string salaryBand, int expiresAt, CappedMobilityOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Team { get; set; } = team;
    public int Attempts { get; set; } = attempts;
    public double Cost { get; set; } = cost;
    public string Region { get; set; } = region;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public CappedMobilityOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a capped mobility override in status idle.</summary>
    public static CappedMobilityOverride NewCappedMobilityOverride(string code, string title)
    {
        return new CappedMobilityOverride(code, title, "", "", 0, 0.0, "", "", 0, CappedMobilityOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedMobilityOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedMobilityOverrideStatus next)
    {
        switch (Status)
        {
            case CappedMobilityOverrideStatus.Idle:
                return next == CappedMobilityOverrideStatus.Warming || next == CappedMobilityOverrideStatus.Stopped;
            case CappedMobilityOverrideStatus.Warming:
                switch (next)
                {
                    case CappedMobilityOverrideStatus.Draining:
                        return true;
                }
                return false;
            case CappedMobilityOverrideStatus.Ready:
                return next == CappedMobilityOverrideStatus.Draining || next == CappedMobilityOverrideStatus.Stopped;
            case CappedMobilityOverrideStatus.Draining:
                return next == CappedMobilityOverrideStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedMobilityOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped mobility override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)ExpiresAt + (double)Attempts, 2);
    }

    public CappedMobilityOverride Copy()
    {
        return new CappedMobilityOverride(
            Code,
            Title,
            Skill,
            Team,
            Attempts,
            Cost,
            Region,
            SalaryBand,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
