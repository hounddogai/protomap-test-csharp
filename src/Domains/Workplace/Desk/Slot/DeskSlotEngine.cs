namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk slot calculation.</summary>
public interface IDeskSlotAdjuster
{
    string Describe();
    double Adjust(DeskSlot item, double valueValue);
}

public sealed record MinimumDeskSlotAdjuster(double Minimum) : IDeskSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDeskSlotAdjuster(List<double> Bounds, List<double> Rates) : IDeskSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskSlot item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedDeskSlotAdjuster(Registry Flags, string Flag, double Percent) : IDeskSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeskSlotRateOffsetAdjuster(double Weight) : IDeskSlotAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(DeskSlot item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingDeskSlotAdjuster(double Step) : IDeskSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDeskSlotAdjuster(double Cap) : IDeskSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the desk slot steps in order and keeps a trace of each.</summary>
public sealed class DeskSlotEngine(List<IDeskSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskSlotEngine DefaultDeskSlotEngine(Registry flags)
    {
        return new DeskSlotEngine(
            new List<IDeskSlotAdjuster>
            {
                new MinimumDeskSlotAdjuster(44.5),
                new TieredDeskSlotAdjuster(
                    new List<double> { 1289.0, 2765.0, 6655.0, 7253.0 },
                    new List<double> { 1.19, 0.87, 1.13, 1.08 }),
                new FlaggedDeskSlotAdjuster(flags, "desk-slot-lenient", 8.9),
                new DeskSlotRateOffsetAdjuster(0.85),
                new RoundingDeskSlotAdjuster(0.5),
                new CapDeskSlotAdjuster(6036.0),
            },
            new List<string>(),
            2.2,
            92535.3);
    }

    public double Compute(DeskSlot item)
    {
        var current = item.NetLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<DeskSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
