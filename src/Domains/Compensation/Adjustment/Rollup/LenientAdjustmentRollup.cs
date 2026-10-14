namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient adjustment rollup is in its life cycle.</summary>
public enum LenientAdjustmentRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient adjustment rollup. Its code is unique within a tenant.</summary>
public sealed class LenientAdjustmentRollup(string code, string title, int quantity, string country, string ownerId, int sequence, double rate, string segment, string channel, LenientAdjustmentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Country { get; set; } = country;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public string Segment { get; set; } = segment;
    public string Channel { get; set; } = channel;
    public LenientAdjustmentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a lenient adjustment rollup in status planned.</summary>
    public static LenientAdjustmentRollup NewLenientAdjustmentRollup(string code, string title)
    {
        return new LenientAdjustmentRollup(
            code,
            title,
            0,
            "",
            "",
            0,
            0.0,
            "",
            "",
            LenientAdjustmentRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientAdjustmentRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientAdjustmentRollupStatus next)
    {
        switch (Status)
        {
            case LenientAdjustmentRollupStatus.Planned:
                return next == LenientAdjustmentRollupStatus.InProgress || next == LenientAdjustmentRollupStatus.Cancelled;
            case LenientAdjustmentRollupStatus.Scheduled:
                switch (next)
                {
                    case LenientAdjustmentRollupStatus.InProgress:
                        return true;
                }
                return false;
            case LenientAdjustmentRollupStatus.InProgress:
                return next == LenientAdjustmentRollupStatus.Completed || next == LenientAdjustmentRollupStatus.Cancelled;
            case LenientAdjustmentRollupStatus.Completed:
                return next == LenientAdjustmentRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientAdjustmentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient adjustment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rate + (double)Sequence, 2);
    }

    public LenientAdjustmentRollup Copy()
    {
        return new LenientAdjustmentRollup(
            Code,
            Title,
            Quantity,
            Country,
            OwnerId,
            Sequence,
            Rate,
            Segment,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
