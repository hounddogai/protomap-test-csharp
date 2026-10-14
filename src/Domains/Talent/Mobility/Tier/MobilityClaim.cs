namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility claim is in its life cycle.</summary>
public enum MobilityClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A mobility claim. Its code is unique within a tenant.</summary>
public sealed class MobilityClaim(string code, string name, bool approved, string locale, double maximum, int quantity, string category, int attempts, int batchSize, double hours, int startsAt, MobilityClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string Locale { get; set; } = locale;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public int BatchSize { get; set; } = batchSize;
    public double Hours { get; set; } = hours;
    public int StartsAt { get; set; } = startsAt;
    public MobilityClaimStatus Status { get; set; } = status;

    /// <summary>Starts a mobility claim in status open.</summary>
    public static MobilityClaim NewMobilityClaim(string code, string name)
    {
        return new MobilityClaim(code, name, false, "", 0.0, 0, "", 0, 0, 0.0, 0, MobilityClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityClaimStatus next)
    {
        switch (Status)
        {
            case MobilityClaimStatus.Open:
                switch (next)
                {
                    case MobilityClaimStatus.Resolved:
                        return true;
                }
                return false;
            case MobilityClaimStatus.InReview:
                return next == MobilityClaimStatus.Reopened;
            case MobilityClaimStatus.Resolved:
                switch (next)
                {
                    case MobilityClaimStatus.Archived:
                        return true;
                }
                return false;
            case MobilityClaimStatus.Reopened:
                switch (next)
                {
                    case MobilityClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Hours * Maximum, 2);
    }

    public MobilityClaim Copy()
    {
        return new MobilityClaim(
            Code,
            Name,
            Approved,
            Locale,
            Maximum,
            Quantity,
            Category,
            Attempts,
            BatchSize,
            Hours,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
