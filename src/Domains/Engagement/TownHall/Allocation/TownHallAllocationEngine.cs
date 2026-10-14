namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall allocation calculation.</summary>
public interface ITownHallAllocationFactor
{
    string Describe();
    double Adjust(TownHallAllocation item, double valueValue);
}

public sealed record TieredTownHallAllocationAdjuster(List<double> Bounds, List<double> Rates) : ITownHallAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallAllocation item, double valueValue)
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

public sealed record FlaggedTownHallAllocationAdjuster(Registry Flags, string Flag, double Percent) : ITownHallAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTownHallAllocationAdjuster(double Cap) : ITownHallAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTownHallAllocationAdjuster(double Step) : ITownHallAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TownHallAllocationRatingOffsetAdjuster(double Weight) : ITownHallAllocationFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(TownHallAllocation item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the town hall allocation steps in order and keeps a trace of each.</summary>
public sealed class TownHallAllocationEngine(List<ITownHallAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallAllocationEngine DefaultTownHallAllocationEngine(Registry flags)
    {
        return new TownHallAllocationEngine(
            new List<ITownHallAllocationFactor>
            {
                new TieredTownHallAllocationAdjuster(
                    new List<double> { 3998.0, 6367.0 },
                    new List<double> { 0.99, 0.91 }),
                new FlaggedTownHallAllocationAdjuster(flags, "town-hall-allocation-manual", 9.9),
                new CapTownHallAllocationAdjuster(47442.0),
                new RoundingTownHallAllocationAdjuster(1.0),
                new TownHallAllocationRatingOffsetAdjuster(1.43),
            },
            new List<string>(),
            1.4,
            33569.7);
    }

    public double Price(TownHallAllocation item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
