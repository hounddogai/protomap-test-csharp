namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive town hall tier is in its life cycle.</summary>
public enum AdaptiveTownHallTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive town hall tier. Its code is unique within a tenant.</summary>
public sealed class AdaptiveTownHallTier(string code, string title, string region, double hours, double budget, int capacity, int quantity, int tenureMonths, int priority, double minimum, AdaptiveTownHallTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public double Hours { get; set; } = hours;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Priority { get; set; } = priority;
    public double Minimum { get; set; } = minimum;
    public AdaptiveTownHallTierStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive town hall tier in status proposed.</summary>
    public static AdaptiveTownHallTier NewAdaptiveTownHallTier(string code, string title)
    {
        return new AdaptiveTownHallTier(
            code,
            title,
            "",
            0.0,
            0.0,
            0,
            0,
            0,
            0,
            0.0,
            AdaptiveTownHallTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTownHallTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTownHallTierStatus next)
    {
        switch (Status)
        {
            case AdaptiveTownHallTierStatus.Proposed:
                switch (next)
                {
                    case AdaptiveTownHallTierStatus.Rejected:
                    case AdaptiveTownHallTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case AdaptiveTownHallTierStatus.Approved:
                return next == AdaptiveTownHallTierStatus.Withdrawn;
            case AdaptiveTownHallTierStatus.Rejected:
                return next == AdaptiveTownHallTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveTownHallTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive town hall tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Minimum + Budget, 2);
    }

    public AdaptiveTownHallTier Copy()
    {
        return new AdaptiveTownHallTier(
            Code,
            Title,
            Region,
            Hours,
            Budget,
            Capacity,
            Quantity,
            TenureMonths,
            Priority,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
