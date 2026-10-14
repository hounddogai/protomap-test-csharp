namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional access badge threshold is in its life cycle.</summary>
public enum RegionalAccessBadgeThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One regional access badge threshold, with what the planners need to act on it.</summary>
public sealed class RegionalAccessBadgeThreshold(string code, string name, string currency, string unit, string notes, double weight, string segment, int durationMinutes, string team, double floorValue, int batchSize, RegionalAccessBadgeThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public double Weight { get; set; } = weight;
    public string Segment { get; set; } = segment;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Team { get; set; } = team;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public RegionalAccessBadgeThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a regional access badge threshold in status draft.</summary>
    public static RegionalAccessBadgeThreshold NewRegionalAccessBadgeThreshold(string code, string name)
    {
        return new RegionalAccessBadgeThreshold(
            code,
            name,
            "",
            "",
            "",
            0.0,
            "",
            0,
            "",
            0.0,
            0,
            RegionalAccessBadgeThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAccessBadgeThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAccessBadgeThresholdStatus next)
    {
        switch (Status)
        {
            case RegionalAccessBadgeThresholdStatus.Draft:
                return next == RegionalAccessBadgeThresholdStatus.Suspended || next == RegionalAccessBadgeThresholdStatus.Closed;
            case RegionalAccessBadgeThresholdStatus.Pending:
                return next == RegionalAccessBadgeThresholdStatus.Active;
            case RegionalAccessBadgeThresholdStatus.Active:
                return next == RegionalAccessBadgeThresholdStatus.Closed;
            case RegionalAccessBadgeThresholdStatus.Suspended:
                return next == RegionalAccessBadgeThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalAccessBadgeThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional access badge threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional access badge threshold"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)BatchSize + FloorValue, 2);
    }

    public RegionalAccessBadgeThreshold Copy()
    {
        return new RegionalAccessBadgeThreshold(
            Code,
            Name,
            Currency,
            Unit,
            Notes,
            Weight,
            Segment,
            DurationMinutes,
            Team,
            FloorValue,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
