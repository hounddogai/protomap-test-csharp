namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional desk mapping is in its life cycle.</summary>
public enum RegionalDeskMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional desk mapping as the desk team keeps it.</summary>
public sealed class RegionalDeskMapping(string code, string title, string ownerId, double rate, string country, int limit, string channel, int endsAt, List<string> tags, RegionalDeskMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Rate { get; set; } = rate;
    public string Country { get; set; } = country;
    public int Limit { get; set; } = limit;
    public string Channel { get; set; } = channel;
    public int EndsAt { get; set; } = endsAt;
    public List<string> Tags { get; set; } = tags;
    public RegionalDeskMappingStatus Status { get; set; } = status;

    /// <summary>Starts a regional desk mapping in status open.</summary>
    public static RegionalDeskMapping NewRegionalDeskMapping(string code, string title)
    {
        return new RegionalDeskMapping(
            code,
            title,
            "",
            0.0,
            "",
            0,
            "",
            0,
            new List<string>(),
            RegionalDeskMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeskMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeskMappingStatus next)
    {
        switch (Status)
        {
            case RegionalDeskMappingStatus.Open:
                switch (next)
                {
                    case RegionalDeskMappingStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalDeskMappingStatus.InReview:
                switch (next)
                {
                    case RegionalDeskMappingStatus.Archived:
                        return true;
                }
                return false;
            case RegionalDeskMappingStatus.Resolved:
                switch (next)
                {
                    case RegionalDeskMappingStatus.Archived:
                        return true;
                }
                return false;
            case RegionalDeskMappingStatus.Reopened:
                switch (next)
                {
                    case RegionalDeskMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalDeskMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional desk mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"regional desk mapping"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedLoad()
    {
        return Math.Round(Rate + (double)Limit, 2);
    }

    public RegionalDeskMapping Copy()
    {
        return new RegionalDeskMapping(Code, Title, OwnerId, Rate, Country, Limit, Channel, EndsAt, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
