namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop tier is in its life cycle.</summary>
public enum LaptopTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One laptop tier, with what the planners need to act on it.</summary>
public sealed class LaptopTier(string code, string title, string region, int revision, double ratio, int durationMinutes, string segment, double floorValue, int limit, LaptopTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public LaptopTierStatus Status { get; set; } = status;

    /// <summary>Starts a laptop tier in status proposed.</summary>
    public static LaptopTier NewLaptopTier(string code, string title)
    {
        return new LaptopTier(code, title, "", 0, 0.0, 0, "", 0.0, 0, LaptopTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopTierStatus next)
    {
        switch (Status)
        {
            case LaptopTierStatus.Proposed:
                return next == LaptopTierStatus.Rejected;
            case LaptopTierStatus.Approved:
                return next == LaptopTierStatus.Withdrawn;
            case LaptopTierStatus.Rejected:
                return next == LaptopTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LaptopTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue - (double)DurationMinutes, 2);
    }

    public LaptopTier Copy()
    {
        return new LaptopTier(
            Code,
            Title,
            Region,
            Revision,
            Ratio,
            DurationMinutes,
            Segment,
            FloorValue,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
