namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback occupancy adjustment is in its life cycle.</summary>
public enum FallbackOccupancyAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One fallback occupancy adjustment, with what the planners need to act on it.</summary>
public sealed class FallbackOccupancyAdjustment(string code, string label, double hours, int quantity, int startsAt, int endsAt, double floorValue, bool visible, FallbackOccupancyAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int Quantity { get; set; } = quantity;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public FallbackOccupancyAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback occupancy adjustment in status new.</summary>
    public static FallbackOccupancyAdjustment NewFallbackOccupancyAdjustment(string code, string label)
    {
        return new FallbackOccupancyAdjustment(
            code,
            label,
            0.0,
            0,
            0,
            0,
            0.0,
            false,
            FallbackOccupancyAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackOccupancyAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackOccupancyAdjustmentStatus next)
    {
        switch (Status)
        {
            case FallbackOccupancyAdjustmentStatus.New:
                return next == FallbackOccupancyAdjustmentStatus.Queued || next == FallbackOccupancyAdjustmentStatus.Running;
            case FallbackOccupancyAdjustmentStatus.Queued:
                switch (next)
                {
                    case FallbackOccupancyAdjustmentStatus.Running:
                    case FallbackOccupancyAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case FallbackOccupancyAdjustmentStatus.Running:
                switch (next)
                {
                    case FallbackOccupancyAdjustmentStatus.Done:
                    case FallbackOccupancyAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case FallbackOccupancyAdjustmentStatus.Done:
                switch (next)
                {
                    case FallbackOccupancyAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackOccupancyAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback occupancy adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Hours - FloorValue, 2);
    }

    public FallbackOccupancyAdjustment Copy()
    {
        return new FallbackOccupancyAdjustment(
            Code,
            Label,
            Hours,
            Quantity,
            StartsAt,
            EndsAt,
            FloorValue,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
