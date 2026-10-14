namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling town hall threshold is in its life cycle.</summary>
public enum RollingTownHallThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a rolling town hall threshold and its current status.</summary>
public sealed class RollingTownHallThreshold(string code, string title, string source, int version, int limit, string team, RollingTownHallThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public int Limit { get; set; } = limit;
    public string Team { get; set; } = team;
    public RollingTownHallThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a rolling town hall threshold in status requested.</summary>
    public static RollingTownHallThreshold NewRollingTownHallThreshold(string code, string title)
    {
        return new RollingTownHallThreshold(code, title, "", 0, 0, "", RollingTownHallThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingTownHallThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingTownHallThresholdStatus next)
    {
        switch (Status)
        {
            case RollingTownHallThresholdStatus.Requested:
                return next == RollingTownHallThresholdStatus.Revoked || next == RollingTownHallThresholdStatus.Expired;
            case RollingTownHallThresholdStatus.Granted:
                return next == RollingTownHallThresholdStatus.Revoked || next == RollingTownHallThresholdStatus.Expired;
            case RollingTownHallThresholdStatus.Revoked:
                switch (next)
                {
                    case RollingTownHallThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingTownHallThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling town hall threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Limit - (double)Version, 2);
    }

    public RollingTownHallThreshold Copy()
    {
        return new RollingTownHallThreshold(Code, Title, Source, Version, Limit, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
