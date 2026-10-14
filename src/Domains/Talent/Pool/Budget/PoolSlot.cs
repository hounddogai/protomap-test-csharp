namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool slot is in its life cycle.</summary>
public enum PoolSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pool slot. Its code is unique within a tenant.</summary>
public sealed class PoolSlot(string code, string label, string locale, double weight, string managerId, double floorValue, string badge, int sequence, PoolSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public double Weight { get; set; } = weight;
    public string ManagerId { get; set; } = managerId;
    public double FloorValue { get; set; } = floorValue;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public PoolSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pool slot in status planned.</summary>
    public static PoolSlot NewPoolSlot(string code, string label)
    {
        return new PoolSlot(code, label, "", 0.0, "", 0.0, "", 0, PoolSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolSlotStatus next)
    {
        switch (Status)
        {
            case PoolSlotStatus.Planned:
                return next == PoolSlotStatus.Scheduled || next == PoolSlotStatus.InProgress;
            case PoolSlotStatus.Scheduled:
                switch (next)
                {
                    case PoolSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolSlotStatus.InProgress:
                switch (next)
                {
                    case PoolSlotStatus.Completed:
                        return true;
                }
                return false;
            case PoolSlotStatus.Completed:
                switch (next)
                {
                    case PoolSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round(FloorValue * Weight, 2);
    }

    public PoolSlot Copy()
    {
        return new PoolSlot(Code, Label, Locale, Weight, ManagerId, FloorValue, Badge, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
