namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted laptop rollup calculation.</summary>
public interface IWeightedLaptopRollupAdjuster
{
    string Describe();
    double Adjust(WeightedLaptopRollup item, double valueValue);
}

public sealed record PercentWeightedLaptopRollupAdjuster(double Percent) : IWeightedLaptopRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedLaptopRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameWeightedLaptopRollupAdjuster(Dictionary<string, double> Factors) : IWeightedLaptopRollupAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedLaptopRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedLaptopRollupAdjuster(double Cap) : IWeightedLaptopRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedLaptopRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWeightedLaptopRollupAdjuster(List<double> Bounds, List<double> Rates) : IWeightedLaptopRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedLaptopRollup item, double valueValue)
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

public sealed record FlaggedWeightedLaptopRollupAdjuster(Registry Flags, string Flag, double Percent) : IWeightedLaptopRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedLaptopRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted laptop rollup steps in order and keeps a trace of each.</summary>
public sealed class WeightedLaptopRollupEngine(List<IWeightedLaptopRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedLaptopRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedLaptopRollupEngine DefaultWeightedLaptopRollupEngine(Registry flags)
    {
        return new WeightedLaptopRollupEngine(
            new List<IWeightedLaptopRollupAdjuster>
            {
                new PercentWeightedLaptopRollupAdjuster(-1.4),
                new NameWeightedLaptopRollupAdjuster(new Dictionary<string, double> { ["east"] = 1.25 }),
                new CapWeightedLaptopRollupAdjuster(42313.0),
                new TieredWeightedLaptopRollupAdjuster(
                    new List<double> { 1674.0, 3334.0 },
                    new List<double> { 1.02, 0.95 }),
                new FlaggedWeightedLaptopRollupAdjuster(flags, "weighted-laptop-rollup-fallback", -8.3),
            },
            new List<string>(),
            0.8,
            3639.0);
    }

    public double Run(WeightedLaptopRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedLaptopRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
