namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped merit cycle score is in its life cycle.</summary>
public enum CappedMeritCycleScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped merit cycle score as the merit cycle team keeps it.</summary>
public sealed class CappedMeritCycleScore(string code, string title, string ownerId, int startsAt, double weight, int version, string region, string locale, string location, CappedMeritCycleScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public string Locale { get; set; } = locale;
    public string Location { get; set; } = location;
    public CappedMeritCycleScoreStatus Status { get; set; } = status;

    /// <summary>Starts a capped merit cycle score in status requested.</summary>
    public static CappedMeritCycleScore NewCappedMeritCycleScore(string code, string title)
    {
        return new CappedMeritCycleScore(
            code,
            title,
            "",
            0,
            0.0,
            0,
            "",
            "",
            "",
            CappedMeritCycleScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedMeritCycleScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedMeritCycleScoreStatus next)
    {
        switch (Status)
        {
            case CappedMeritCycleScoreStatus.Requested:
                return next == CappedMeritCycleScoreStatus.Revoked;
            case CappedMeritCycleScoreStatus.Granted:
                return next == CappedMeritCycleScoreStatus.Revoked;
            case CappedMeritCycleScoreStatus.Revoked:
                return next == CappedMeritCycleScoreStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedMeritCycleScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped merit cycle score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Version * (double)StartsAt, 2);
    }

    public CappedMeritCycleScore Copy()
    {
        return new CappedMeritCycleScore(
            Code,
            Title,
            OwnerId,
            StartsAt,
            Weight,
            Version,
            Region,
            Locale,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
