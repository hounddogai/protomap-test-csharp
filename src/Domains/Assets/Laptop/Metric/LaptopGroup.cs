namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop group is in its life cycle.</summary>
public enum LaptopGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One laptop group, with what the planners need to act on it.</summary>
public sealed class LaptopGroup(string code, string name, double ratio, double floorValue, string timezone, LaptopGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public double FloorValue { get; set; } = floorValue;
    public string Timezone { get; set; } = timezone;
    public LaptopGroupStatus Status { get; set; } = status;

    /// <summary>Starts a laptop group in status proposed.</summary>
    public static LaptopGroup NewLaptopGroup(string code, string name)
    {
        return new LaptopGroup(code, name, 0.0, 0.0, "", LaptopGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopGroupStatus next)
    {
        switch (Status)
        {
            case LaptopGroupStatus.Proposed:
                switch (next)
                {
                    case LaptopGroupStatus.Rejected:
                    case LaptopGroupStatus.Withdrawn:
                        return true;
                }
                return false;
            case LaptopGroupStatus.Approved:
                switch (next)
                {
                    case LaptopGroupStatus.Rejected:
                        return true;
                }
                return false;
            case LaptopGroupStatus.Rejected:
                return next == LaptopGroupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LaptopGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"laptop group"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Ratio - FloorValue, 2);
    }

    public LaptopGroup Copy()
    {
        return new LaptopGroup(Code, Name, Ratio, FloorValue, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
