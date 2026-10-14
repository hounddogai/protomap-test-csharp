namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal adjustment segment is in its life cycle.</summary>
public enum SeasonalAdjustmentSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal adjustment segment as the adjustment team keeps it.</summary>
public sealed class SeasonalAdjustmentSegment(string code, string label, double amount, double cost, int startsAt, string team, SeasonalAdjustmentSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public double Cost { get; set; } = cost;
    public int StartsAt { get; set; } = startsAt;
    public string Team { get; set; } = team;
    public SeasonalAdjustmentSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal adjustment segment in status planned.</summary>
    public static SeasonalAdjustmentSegment NewSeasonalAdjustmentSegment(string code, string label)
    {
        return new SeasonalAdjustmentSegment(code, label, 0.0, 0.0, 0, "", SeasonalAdjustmentSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAdjustmentSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAdjustmentSegmentStatus next)
    {
        switch (Status)
        {
            case SeasonalAdjustmentSegmentStatus.Planned:
                return next == SeasonalAdjustmentSegmentStatus.Scheduled;
            case SeasonalAdjustmentSegmentStatus.Scheduled:
                return next == SeasonalAdjustmentSegmentStatus.InProgress;
            case SeasonalAdjustmentSegmentStatus.InProgress:
                switch (next)
                {
                    case SeasonalAdjustmentSegmentStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalAdjustmentSegmentStatus.Completed:
                return next == SeasonalAdjustmentSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SeasonalAdjustmentSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal adjustment segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost + Amount, 2);
    }

    public SeasonalAdjustmentSegment Copy()
    {
        return new SeasonalAdjustmentSegment(Code, Label, Amount, Cost, StartsAt, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
