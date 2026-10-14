namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop slot is in its life cycle.</summary>
public enum LaptopSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One laptop slot, with what the planners need to act on it.</summary>
public sealed class LaptopSlot(string code, string title, double ratio, double cost, int quantity, string costCenter, string category, string unit, string country, double minimum, string source, LaptopSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public int Quantity { get; set; } = quantity;
    public string CostCenter { get; set; } = costCenter;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public string Country { get; set; } = country;
    public double Minimum { get; set; } = minimum;
    public string Source { get; set; } = source;
    public LaptopSlotStatus Status { get; set; } = status;

    /// <summary>Starts a laptop slot in status new.</summary>
    public static LaptopSlot NewLaptopSlot(string code, string title)
    {
        return new LaptopSlot(code, title, 0.0, 0.0, 0, "", "", "", "", 0.0, "", LaptopSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopSlotStatus next)
    {
        switch (Status)
        {
            case LaptopSlotStatus.New:
                return next == LaptopSlotStatus.Queued || next == LaptopSlotStatus.Running;
            case LaptopSlotStatus.Queued:
                return next == LaptopSlotStatus.Running || next == LaptopSlotStatus.Done;
            case LaptopSlotStatus.Running:
                return next == LaptopSlotStatus.Done || next == LaptopSlotStatus.Failed;
            case LaptopSlotStatus.Done:
                switch (next)
                {
                    case LaptopSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Cost + Minimum, 2);
    }

    public LaptopSlot Copy()
    {
        return new LaptopSlot(
            Code,
            Title,
            Ratio,
            Cost,
            Quantity,
            CostCenter,
            Category,
            Unit,
            Country,
            Minimum,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
