namespace Acme.Workforce.Engagement.TownHall.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall quota is in its life cycle.</summary>
public enum TownHallQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One town hall quota, with what the planners need to act on it.</summary>
public sealed class TownHallQuota(string code, string title, double discount, int version, string skill, int endsAt, int windowDays, string team, string country, TownHallQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public int Version { get; set; } = version;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public int WindowDays { get; set; } = windowDays;
    public string Team { get; set; } = team;
    public string Country { get; set; } = country;
    public TownHallQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a town hall quota in status draft.</summary>
    public static TownHallQuota NewTownHallQuota(string code, string title)
    {
        return new TownHallQuota(code, title, 0.0, 0, "", 0, 0, "", "", TownHallQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallQuotaStatus next)
    {
        switch (Status)
        {
            case TownHallQuotaStatus.Draft:
                return next == TownHallQuotaStatus.Active || next == TownHallQuotaStatus.Suspended;
            case TownHallQuotaStatus.Pending:
                switch (next)
                {
                    case TownHallQuotaStatus.Active:
                    case TownHallQuotaStatus.Closed:
                        return true;
                }
                return false;
            case TownHallQuotaStatus.Active:
                return next == TownHallQuotaStatus.Suspended || next == TownHallQuotaStatus.Closed;
            case TownHallQuotaStatus.Suspended:
                switch (next)
                {
                    case TownHallQuotaStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round(Discount + (double)EndsAt, 2);
    }

    public TownHallQuota Copy()
    {
        return new TownHallQuota(Code, Title, Discount, Version, Skill, EndsAt, WindowDays, Team, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
