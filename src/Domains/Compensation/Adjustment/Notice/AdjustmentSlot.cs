namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment slot is in its life cycle.</summary>
public enum AdjustmentSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adjustment slot and its current status.</summary>
public sealed class AdjustmentSlot(string code, string title, int windowDays, double rating, int graceDays, int createdAt, double discount, AdjustmentSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public double Rating { get; set; } = rating;
    public int GraceDays { get; set; } = graceDays;
    public int CreatedAt { get; set; } = createdAt;
    public double Discount { get; set; } = discount;
    public AdjustmentSlotStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment slot in status idle.</summary>
    public static AdjustmentSlot NewAdjustmentSlot(string code, string title)
    {
        return new AdjustmentSlot(code, title, 0, 0.0, 0, 0, 0.0, AdjustmentSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentSlotStatus next)
    {
        switch (Status)
        {
            case AdjustmentSlotStatus.Idle:
                switch (next)
                {
                    case AdjustmentSlotStatus.Draining:
                        return true;
                }
                return false;
            case AdjustmentSlotStatus.Warming:
                return next == AdjustmentSlotStatus.Stopped;
            case AdjustmentSlotStatus.Ready:
                return next == AdjustmentSlotStatus.Stopped;
            case AdjustmentSlotStatus.Draining:
                switch (next)
                {
                    case AdjustmentSlotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adjustment slot"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rating - Discount, 2);
    }

    public AdjustmentSlot Copy()
    {
        return new AdjustmentSlot(Code, Title, WindowDays, Rating, GraceDays, CreatedAt, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
