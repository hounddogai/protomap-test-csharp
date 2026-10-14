namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global span rollup calculation.</summary>
public interface IGlobalSpanRollupStep
{
    string Describe();
    double Adjust(GlobalSpanRollup item, double valueValue);
}

public sealed record RoundingGlobalSpanRollupAdjuster(double Step) : IGlobalSpanRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalSpanRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record GlobalSpanRollupQuantityOffsetAdjuster(double Weight) : IGlobalSpanRollupStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(GlobalSpanRollup item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapGlobalSpanRollupAdjuster(double Cap) : IGlobalSpanRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalSpanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelGlobalSpanRollupAdjuster(Dictionary<string, double> Factors) : IGlobalSpanRollupStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(GlobalSpanRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalSpanRollupAdjuster(double Minimum) : IGlobalSpanRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalSpanRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGlobalSpanRollupAdjuster(Registry Flags, string Flag, double Percent) : IGlobalSpanRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalSpanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global span rollup steps in order and keeps a trace of each.</summary>
public sealed class GlobalSpanRollupEngine(List<IGlobalSpanRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalSpanRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalSpanRollupEngine DefaultGlobalSpanRollupEngine(Registry flags)
    {
        return new GlobalSpanRollupEngine(
            new List<IGlobalSpanRollupStep>
            {
                new RoundingGlobalSpanRollupAdjuster(0.1),
                new GlobalSpanRollupQuantityOffsetAdjuster(0.33),
                new CapGlobalSpanRollupAdjuster(42426.0),
                new ChannelGlobalSpanRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.37, ["central"] = 1.38 }),
                new MinimumGlobalSpanRollupAdjuster(42.2),
                new FlaggedGlobalSpanRollupAdjuster(flags, "global-span-rollup-manual", 9.5),
            },
            new List<string>(),
            0.4,
            33188.1);
    }

    public double Compute(GlobalSpanRollup item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalSpanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
