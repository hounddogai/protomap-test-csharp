namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling floor plan slot is in its life cycle.</summary>
public enum RollingFloorPlanSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling floor plan slot. Its code is unique within a tenant.</summary>
public sealed class RollingFloorPlanSlot(string code, string title, string currency, int createdAt, bool active, double ratio, string category, RollingFloorPlanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public bool Active { get; set; } = active;
    public double Ratio { get; set; } = ratio;
    public string Category { get; set; } = category;
    public RollingFloorPlanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a rolling floor plan slot in status planned.</summary>
    public static RollingFloorPlanSlot NewRollingFloorPlanSlot(string code, string title)
    {
        return new RollingFloorPlanSlot(code, title, "", 0, false, 0.0, "", RollingFloorPlanSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFloorPlanSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFloorPlanSlotStatus next)
    {
        switch (Status)
        {
            case RollingFloorPlanSlotStatus.Planned:
                switch (next)
                {
                    case RollingFloorPlanSlotStatus.Completed:
                    case RollingFloorPlanSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingFloorPlanSlotStatus.Scheduled:
                switch (next)
                {
                    case RollingFloorPlanSlotStatus.InProgress:
                    case RollingFloorPlanSlotStatus.Completed:
                        return true;
                }
                return false;
            case RollingFloorPlanSlotStatus.InProgress:
                return next == RollingFloorPlanSlotStatus.Cancelled;
            case RollingFloorPlanSlotStatus.Completed:
                switch (next)
                {
                    case RollingFloorPlanSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingFloorPlanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling floor plan slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)CreatedAt - Ratio, 2);
    }

    public RollingFloorPlanSlot Copy()
    {
        return new RollingFloorPlanSlot(Code, Title, Currency, CreatedAt, Active, Ratio, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
