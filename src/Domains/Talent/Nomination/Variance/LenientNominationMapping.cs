namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient nomination mapping is in its life cycle.</summary>
public enum LenientNominationMappingStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient nomination mapping as the nomination team keeps it.</summary>
public sealed class LenientNominationMapping(string code, string name, bool archived, string source, string currency, int windowDays, double weight, string badge, double budget, string team, int createdAt, LenientNominationMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public string Currency { get; set; } = currency;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public double Budget { get; set; } = budget;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public LenientNominationMappingStatus Status { get; set; } = status;

    /// <summary>Starts a lenient nomination mapping in status created.</summary>
    public static LenientNominationMapping NewLenientNominationMapping(string code, string name)
    {
        return new LenientNominationMapping(
            code,
            name,
            false,
            "",
            "",
            0,
            0.0,
            "",
            0.0,
            "",
            0,
            LenientNominationMappingStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientNominationMappingStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientNominationMappingStatus next)
    {
        switch (Status)
        {
            case LenientNominationMappingStatus.Created:
                return next == LenientNominationMappingStatus.Validated || next == LenientNominationMappingStatus.Retired;
            case LenientNominationMappingStatus.Validated:
                return next == LenientNominationMappingStatus.Published;
            case LenientNominationMappingStatus.Published:
                return next == LenientNominationMappingStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LenientNominationMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient nomination mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt - (double)WindowDays, 2);
    }

    public LenientNominationMapping Copy()
    {
        return new LenientNominationMapping(
            Code,
            Name,
            Archived,
            Source,
            Currency,
            WindowDays,
            Weight,
            Badge,
            Budget,
            Team,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
