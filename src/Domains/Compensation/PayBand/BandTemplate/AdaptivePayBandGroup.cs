namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive pay band group is in its life cycle.</summary>
public enum AdaptivePayBandGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive pay band group as the pay band team keeps it.</summary>
public sealed class AdaptivePayBandGroup(string code, string title, string region, bool approved, int quantity, string timezone, int capacity, double threshold, AdaptivePayBandGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string Timezone { get; set; } = timezone;
    public int Capacity { get; set; } = capacity;
    public double Threshold { get; set; } = threshold;
    public AdaptivePayBandGroupStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive pay band group in status draft.</summary>
    public static AdaptivePayBandGroup NewAdaptivePayBandGroup(string code, string title)
    {
        return new AdaptivePayBandGroup(code, title, "", false, 0, "", 0, 0.0, AdaptivePayBandGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePayBandGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePayBandGroupStatus next)
    {
        switch (Status)
        {
            case AdaptivePayBandGroupStatus.Draft:
                switch (next)
                {
                    case AdaptivePayBandGroupStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptivePayBandGroupStatus.Pending:
                switch (next)
                {
                    case AdaptivePayBandGroupStatus.Closed:
                        return true;
                }
                return false;
            case AdaptivePayBandGroupStatus.Active:
                return next == AdaptivePayBandGroupStatus.Suspended;
            case AdaptivePayBandGroupStatus.Suspended:
                switch (next)
                {
                    case AdaptivePayBandGroupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePayBandGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive pay band group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive pay band group"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity - Threshold, 2);
    }

    public AdaptivePayBandGroup Copy()
    {
        return new AdaptivePayBandGroup(
            Code,
            Title,
            Region,
            Approved,
            Quantity,
            Timezone,
            Capacity,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
