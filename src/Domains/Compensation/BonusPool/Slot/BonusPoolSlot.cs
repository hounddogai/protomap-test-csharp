namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool slot is in its life cycle.</summary>
public enum BonusPoolSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A bonus pool slot. Its code is unique within a tenant.</summary>
public sealed class BonusPoolSlot(string code, string name, int windowDays, bool approved, double budget, int priority, List<string> tags, int version, double floorValue, string notes, BonusPoolSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public bool Approved { get; set; } = approved;
    public double Budget { get; set; } = budget;
    public int Priority { get; set; } = priority;
    public List<string> Tags { get; set; } = tags;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public string Notes { get; set; } = notes;
    public BonusPoolSlotStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool slot in status created.</summary>
    public static BonusPoolSlot NewBonusPoolSlot(string code, string name)
    {
        return new BonusPoolSlot(
            code,
            name,
            0,
            false,
            0.0,
            0,
            new List<string>(),
            0,
            0.0,
            "",
            BonusPoolSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolSlotStatus next)
    {
        switch (Status)
        {
            case BonusPoolSlotStatus.Created:
                return next == BonusPoolSlotStatus.Validated || next == BonusPoolSlotStatus.Retired;
            case BonusPoolSlotStatus.Validated:
                switch (next)
                {
                    case BonusPoolSlotStatus.Published:
                    case BonusPoolSlotStatus.Retired:
                        return true;
                }
                return false;
            case BonusPoolSlotStatus.Published:
                switch (next)
                {
                    case BonusPoolSlotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"bonus pool slot"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetExposure()
    {
        return Math.Round((double)Priority + (double)WindowDays, 2);
    }

    public BonusPoolSlot Copy()
    {
        return new BonusPoolSlot(
            Code,
            Name,
            WindowDays,
            Approved,
            Budget,
            Priority,
            Tags,
            Version,
            FloorValue,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
