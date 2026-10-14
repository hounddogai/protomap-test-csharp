namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility entry is in its life cycle.</summary>
public enum MobilityEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One mobility entry, with what the planners need to act on it.</summary>
public sealed class MobilityEntry(string code, string name, double budget, string country, int endsAt, string title, string team, MobilityEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Country { get; set; } = country;
    public int EndsAt { get; set; } = endsAt;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public MobilityEntryStatus Status { get; set; } = status;

    /// <summary>Starts a mobility entry in status draft.</summary>
    public static MobilityEntry NewMobilityEntry(string code, string name)
    {
        return new MobilityEntry(code, name, 0.0, "", 0, "", "", MobilityEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityEntryStatus next)
    {
        switch (Status)
        {
            case MobilityEntryStatus.Draft:
                return next == MobilityEntryStatus.Pending || next == MobilityEntryStatus.Closed;
            case MobilityEntryStatus.Pending:
                switch (next)
                {
                    case MobilityEntryStatus.Active:
                    case MobilityEntryStatus.Closed:
                        return true;
                }
                return false;
            case MobilityEntryStatus.Active:
                switch (next)
                {
                    case MobilityEntryStatus.Closed:
                        return true;
                }
                return false;
            case MobilityEntryStatus.Suspended:
                switch (next)
                {
                    case MobilityEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * (double)EndsAt, 2);
    }

    public MobilityEntry Copy()
    {
        return new MobilityEntry(Code, Name, Budget, Country, EndsAt, Title, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
