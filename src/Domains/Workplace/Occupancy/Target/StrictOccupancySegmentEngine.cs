namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict occupancy segment calculation.</summary>
public interface IStrictOccupancySegmentAdjuster
{
    string Describe();
    double Adjust(StrictOccupancySegment item, double valueValue);
}

public sealed record FlaggedStrictOccupancySegmentAdjuster(Registry Flags, string Flag, double Percent) : IStrictOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictOccupancySegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictOccupancySegmentAdjuster(double Percent) : IStrictOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictOccupancySegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictOccupancySegmentAdjuster(double Step) : IStrictOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictOccupancySegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStrictOccupancySegmentAdjuster(List<double> Bounds, List<double> Rates) : IStrictOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictOccupancySegment item, double valueValue)
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

public sealed record StrictOccupancySegmentAmountOffsetAdjuster(double Weight) : IStrictOccupancySegmentAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(StrictOccupancySegment item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the strict occupancy segment steps in order and keeps a trace of each.</summary>
public sealed class StrictOccupancySegmentEngine(List<IStrictOccupancySegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictOccupancySegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictOccupancySegmentEngine DefaultStrictOccupancySegmentEngine(Registry flags)
    {
        return new StrictOccupancySegmentEngine(
            new List<IStrictOccupancySegmentAdjuster>
            {
                new FlaggedStrictOccupancySegmentAdjuster(flags, "strict-occupancy-segment-global", 8.0),
                new PercentStrictOccupancySegmentAdjuster(-4.9),
                new RoundingStrictOccupancySegmentAdjuster(1.0),
                new TieredStrictOccupancySegmentAdjuster(
                    new List<double> { 4273.0, 6448.0, 8990.0 },
                    new List<double> { 1.13, 1.18, 1.17 }),
                new StrictOccupancySegmentAmountOffsetAdjuster(1.3),
            },
            new List<string>(),
            0.5,
            47447.6);
    }

    public double Compute(StrictOccupancySegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictOccupancySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
