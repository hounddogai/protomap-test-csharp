namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict mobility override calculation.</summary>
public interface IStrictMobilityOverrideStep
{
    string Describe();
    double Adjust(StrictMobilityOverride item, double valueValue);
}

public sealed record RoundingStrictMobilityOverrideAdjuster(double Step) : IStrictMobilityOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictMobilityOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentStrictMobilityOverrideAdjuster(Dictionary<string, double> Factors) : IStrictMobilityOverrideStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(StrictMobilityOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedStrictMobilityOverrideAdjuster(Registry Flags, string Flag, double Percent) : IStrictMobilityOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictMobilityOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictMobilityOverrideAdjuster(double Minimum) : IStrictMobilityOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictMobilityOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict mobility override steps in order and keeps a trace of each.</summary>
public sealed class StrictMobilityOverrideEngine(List<IStrictMobilityOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictMobilityOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictMobilityOverrideEngine DefaultStrictMobilityOverrideEngine(Registry flags)
    {
        return new StrictMobilityOverrideEngine(
            new List<IStrictMobilityOverrideStep>
            {
                new RoundingStrictMobilityOverrideAdjuster(0.05),
                new SegmentStrictMobilityOverrideAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.27, ["west"] = 1.39 }),
                new FlaggedStrictMobilityOverrideAdjuster(flags, "strict-mobility-override-seasonal", 3.0),
                new MinimumStrictMobilityOverrideAdjuster(34.0),
            },
            new List<string>(),
            2.6,
            10051.7);
    }

    public double Price(StrictMobilityOverride item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictMobilityOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
