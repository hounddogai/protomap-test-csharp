namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped laptop rollup is in its life cycle.</summary>
public enum CappedLaptopRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped laptop rollup, with what the planners need to act on it.</summary>
public sealed class CappedLaptopRollup(string code, string title, string badge, string name, bool pinned, double weight, bool visible, string notes, double floorValue, bool active, CappedLaptopRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public double Weight { get; set; } = weight;
    public bool Visible { get; set; } = visible;
    public string Notes { get; set; } = notes;
    public double FloorValue { get; set; } = floorValue;
    public bool Active { get; set; } = active;
    public CappedLaptopRollupStatus Status { get; set; } = status;

    /// <summary>Starts a capped laptop rollup in status proposed.</summary>
    public static CappedLaptopRollup NewCappedLaptopRollup(string code, string title)
    {
        return new CappedLaptopRollup(
            code,
            title,
            "",
            "",
            false,
            0.0,
            false,
            "",
            0.0,
            false,
            CappedLaptopRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLaptopRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLaptopRollupStatus next)
    {
        switch (Status)
        {
            case CappedLaptopRollupStatus.Proposed:
                return next == CappedLaptopRollupStatus.Rejected || next == CappedLaptopRollupStatus.Withdrawn;
            case CappedLaptopRollupStatus.Approved:
                switch (next)
                {
                    case CappedLaptopRollupStatus.Rejected:
                    case CappedLaptopRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedLaptopRollupStatus.Rejected:
                switch (next)
                {
                    case CappedLaptopRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedLaptopRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped laptop rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Weight * FloorValue, 2);
    }

    public CappedLaptopRollup Copy()
    {
        return new CappedLaptopRollup(
            Code,
            Title,
            Badge,
            Name,
            Pinned,
            Weight,
            Visible,
            Notes,
            FloorValue,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
