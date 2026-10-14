namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered laptop tier is in its life cycle.</summary>
public enum TieredLaptopTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered laptop tier as the laptop team keeps it.</summary>
public sealed class TieredLaptopTier(string code, string label, int attempts, int version, double rating, string contactEmail, string source, string externalRef, double hours, int startsAt, TieredLaptopTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public double Rating { get; set; } = rating;
    public string ContactEmail { get; set; } = contactEmail;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public double Hours { get; set; } = hours;
    public int StartsAt { get; set; } = startsAt;
    public TieredLaptopTierStatus Status { get; set; } = status;

    /// <summary>Starts a tiered laptop tier in status open.</summary>
    public static TieredLaptopTier NewTieredLaptopTier(string code, string label)
    {
        return new TieredLaptopTier(code, label, 0, 0, 0.0, "", "", "", 0.0, 0, TieredLaptopTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLaptopTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLaptopTierStatus next)
    {
        switch (Status)
        {
            case TieredLaptopTierStatus.Open:
                return next == TieredLaptopTierStatus.InReview || next == TieredLaptopTierStatus.Reopened;
            case TieredLaptopTierStatus.InReview:
                return next == TieredLaptopTierStatus.Reopened || next == TieredLaptopTierStatus.Archived;
            case TieredLaptopTierStatus.Resolved:
                switch (next)
                {
                    case TieredLaptopTierStatus.Reopened:
                    case TieredLaptopTierStatus.Archived:
                        return true;
                }
                return false;
            case TieredLaptopTierStatus.Reopened:
                switch (next)
                {
                    case TieredLaptopTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredLaptopTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered laptop tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"tiered laptop tier"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Version * Hours, 2);
    }

    public TieredLaptopTier Copy()
    {
        return new TieredLaptopTier(
            Code,
            Label,
            Attempts,
            Version,
            Rating,
            ContactEmail,
            Source,
            ExternalRef,
            Hours,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
