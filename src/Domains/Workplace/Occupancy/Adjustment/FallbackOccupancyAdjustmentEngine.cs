namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback occupancy adjustment calculation.</summary>
public interface IFallbackOccupancyAdjustmentFactor
{
    string Describe();
    double Adjust(FallbackOccupancyAdjustment item, double valueValue);
}

public sealed record CapFallbackOccupancyAdjustmentAdjuster(double Cap) : IFallbackOccupancyAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackOccupancyAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackOccupancyAdjustmentFloorValueOffsetAdjuster(double Weight) : IFallbackOccupancyAdjustmentFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(FallbackOccupancyAdjustment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record LabelFallbackOccupancyAdjustmentAdjuster(Dictionary<string, double> Factors) : IFallbackOccupancyAdjustmentFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FallbackOccupancyAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackOccupancyAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IFallbackOccupancyAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackOccupancyAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback occupancy adjustment steps in order and keeps a trace of each.</summary>
public sealed class FallbackOccupancyAdjustmentEngine(List<IFallbackOccupancyAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackOccupancyAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackOccupancyAdjustmentEngine DefaultFallbackOccupancyAdjustmentEngine(Registry flags)
    {
        return new FallbackOccupancyAdjustmentEngine(
            new List<IFallbackOccupancyAdjustmentFactor>
            {
                new CapFallbackOccupancyAdjustmentAdjuster(4967.0),
                new FallbackOccupancyAdjustmentFloorValueOffsetAdjuster(1.97),
                new LabelFallbackOccupancyAdjustmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.91, ["east"] = 1.35 }),
                new FlaggedFallbackOccupancyAdjustmentAdjuster(flags, "fallback-occupancy-adjustment-default", 9.5),
            },
            new List<string>(),
            0.9,
            9442.6);
    }

    public double Price(FallbackOccupancyAdjustment item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackOccupancyAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
