namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility profile is in its life cycle.</summary>
public enum MobilityProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A mobility profile as the mobility team keeps it.</summary>
public sealed class MobilityProfile(string code, string title, int priority, int endsAt, double fte, MobilityProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public double Fte { get; set; } = fte;
    public MobilityProfileStatus Status { get; set; } = status;

    /// <summary>Starts a mobility profile in status draft.</summary>
    public static MobilityProfile NewMobilityProfile(string code, string title)
    {
        return new MobilityProfile(code, title, 0, 0, 0.0, MobilityProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityProfileStatus next)
    {
        switch (Status)
        {
            case MobilityProfileStatus.Draft:
                return next == MobilityProfileStatus.Pending || next == MobilityProfileStatus.Closed;
            case MobilityProfileStatus.Pending:
                switch (next)
                {
                    case MobilityProfileStatus.Closed:
                        return true;
                }
                return false;
            case MobilityProfileStatus.Active:
                return next == MobilityProfileStatus.Suspended;
            case MobilityProfileStatus.Suspended:
                return next == MobilityProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(MobilityProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Fte - (double)Priority, 2);
    }

    public MobilityProfile Copy()
    {
        return new MobilityProfile(Code, Title, Priority, EndsAt, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
