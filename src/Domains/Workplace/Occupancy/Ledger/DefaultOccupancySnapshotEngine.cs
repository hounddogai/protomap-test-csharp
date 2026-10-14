namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default occupancy snapshot calculation.</summary>
public interface IDefaultOccupancySnapshotStep
{
    string Describe();
    double Adjust(DefaultOccupancySnapshot item, double valueValue);
}

public sealed record CategoryDefaultOccupancySnapshotAdjuster(Dictionary<string, double> Factors) : IDefaultOccupancySnapshotStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(DefaultOccupancySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultOccupancySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IDefaultOccupancySnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultOccupancySnapshot item, double valueValue)
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

public sealed record MinimumDefaultOccupancySnapshotAdjuster(double Minimum) : IDefaultOccupancySnapshotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultOccupancySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDefaultOccupancySnapshotAdjuster(double Cap) : IDefaultOccupancySnapshotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultOccupancySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultOccupancySnapshotAdjuster(Registry Flags, string Flag, double Percent) : IDefaultOccupancySnapshotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultOccupancySnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultOccupancySnapshotAdjuster(double Step) : IDefaultOccupancySnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultOccupancySnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default occupancy snapshot steps in order and keeps a trace of each.</summary>
public sealed class DefaultOccupancySnapshotEngine(List<IDefaultOccupancySnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultOccupancySnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultOccupancySnapshotEngine DefaultDefaultOccupancySnapshotEngine(Registry flags)
    {
        return new DefaultOccupancySnapshotEngine(
            new List<IDefaultOccupancySnapshotStep>
            {
                new CategoryDefaultOccupancySnapshotAdjuster(new Dictionary<string, double> { ["east"] = 1.2 }),
                new TieredDefaultOccupancySnapshotAdjuster(
                    new List<double> { 1181.0, 3770.0 },
                    new List<double> { 0.81, 1.19 }),
                new MinimumDefaultOccupancySnapshotAdjuster(38.5),
                new CapDefaultOccupancySnapshotAdjuster(28985.0),
                new FlaggedDefaultOccupancySnapshotAdjuster(flags, "default-occupancy-snapshot-capped", 4.9),
                new RoundingDefaultOccupancySnapshotAdjuster(0.1),
            },
            new List<string>(),
            1.3,
            19468.9);
    }

    public double Price(DefaultOccupancySnapshot item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultOccupancySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
