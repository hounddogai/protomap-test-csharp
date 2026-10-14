namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional desk claim is in its life cycle.</summary>
public enum RegionalDeskClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional desk claim and its current status.</summary>
public sealed class RegionalDeskClaim(string code, string name, string team, int attempts, string notes, int expiresAt, bool pinned, double minimum, List<string> tags, int batchSize, RegionalDeskClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public int Attempts { get; set; } = attempts;
    public string Notes { get; set; } = notes;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public double Minimum { get; set; } = minimum;
    public List<string> Tags { get; set; } = tags;
    public int BatchSize { get; set; } = batchSize;
    public RegionalDeskClaimStatus Status { get; set; } = status;

    /// <summary>Starts a regional desk claim in status created.</summary>
    public static RegionalDeskClaim NewRegionalDeskClaim(string code, string name)
    {
        return new RegionalDeskClaim(
            code,
            name,
            "",
            0,
            "",
            0,
            false,
            0.0,
            new List<string>(),
            0,
            RegionalDeskClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeskClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeskClaimStatus next)
    {
        switch (Status)
        {
            case RegionalDeskClaimStatus.Created:
                return next == RegionalDeskClaimStatus.Validated || next == RegionalDeskClaimStatus.Published;
            case RegionalDeskClaimStatus.Validated:
                switch (next)
                {
                    case RegionalDeskClaimStatus.Published:
                    case RegionalDeskClaimStatus.Retired:
                        return true;
                }
                return false;
            case RegionalDeskClaimStatus.Published:
                return next == RegionalDeskClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalDeskClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional desk claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"regional desk claim"} {Code}";
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

    public double AdjustedTotal()
    {
        return Math.Round((double)Attempts * Minimum, 2);
    }

    public RegionalDeskClaim Copy()
    {
        return new RegionalDeskClaim(
            Code,
            Name,
            Team,
            Attempts,
            Notes,
            ExpiresAt,
            Pinned,
            Minimum,
            Tags,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
