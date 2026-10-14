namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict rate mapping calculation.</summary>
public interface IStrictRateMappingAdjuster
{
    string Describe();
    double Adjust(StrictRateMapping item, double valueValue);
}

public sealed record PercentStrictRateMappingAdjuster(double Percent) : IStrictRateMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictRateMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameStrictRateMappingAdjuster(Dictionary<string, double> Factors) : IStrictRateMappingAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictRateMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StrictRateMappingThresholdOffsetAdjuster(double Weight) : IStrictRateMappingAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(StrictRateMapping item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record CapStrictRateMappingAdjuster(double Cap) : IStrictRateMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictRateMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStrictRateMappingAdjuster(Registry Flags, string Flag, double Percent) : IStrictRateMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictRateMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictRateMappingAdjuster(double Step) : IStrictRateMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictRateMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict rate mapping steps in order and keeps a trace of each.</summary>
public sealed class StrictRateMappingEngine(List<IStrictRateMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictRateMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictRateMappingEngine DefaultStrictRateMappingEngine(Registry flags)
    {
        return new StrictRateMappingEngine(
            new List<IStrictRateMappingAdjuster>
            {
                new PercentStrictRateMappingAdjuster(-13.2),
                new NameStrictRateMappingAdjuster(new Dictionary<string, double> { ["east"] = 0.88 }),
                new StrictRateMappingThresholdOffsetAdjuster(1.59),
                new CapStrictRateMappingAdjuster(36724.0),
                new FlaggedStrictRateMappingAdjuster(flags, "strict-rate-mapping-manual", -1.5),
                new RoundingStrictRateMappingAdjuster(0.25),
            },
            new List<string>(),
            3.4,
            52969.6);
    }

    public double Compute(StrictRateMapping item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictRateMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
