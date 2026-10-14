namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop adjustment is in its life cycle.</summary>
public enum LaptopAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One laptop adjustment, with what the planners need to act on it.</summary>
public sealed class LaptopAdjustment(string code, string title, bool pinned, string phone, double rate, double weight, double maximum, LaptopAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public double Weight { get; set; } = weight;
    public double Maximum { get; set; } = maximum;
    public LaptopAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a laptop adjustment in status planned.</summary>
    public static LaptopAdjustment NewLaptopAdjustment(string code, string title)
    {
        return new LaptopAdjustment(code, title, false, "", 0.0, 0.0, 0.0, LaptopAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopAdjustmentStatus next)
    {
        switch (Status)
        {
            case LaptopAdjustmentStatus.Planned:
                switch (next)
                {
                    case LaptopAdjustmentStatus.Scheduled:
                    case LaptopAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case LaptopAdjustmentStatus.Scheduled:
                return next == LaptopAdjustmentStatus.InProgress || next == LaptopAdjustmentStatus.Completed;
            case LaptopAdjustmentStatus.InProgress:
                return next == LaptopAdjustmentStatus.Cancelled;
            case LaptopAdjustmentStatus.Completed:
                return next == LaptopAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LaptopAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Weight + Rate, 2);
    }

    public LaptopAdjustment Copy()
    {
        return new LaptopAdjustment(Code, Title, Pinned, Phone, Rate, Weight, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
