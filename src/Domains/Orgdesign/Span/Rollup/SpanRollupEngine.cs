namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span rollup calculation.</summary>
public interface ISpanRollupStep
{
    string Describe();
    double Adjust(SpanRollup item, double valueValue);
}

public sealed record PercentSpanRollupAdjuster(double Percent) : ISpanRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSpanRollupAdjuster(double Cap) : ISpanRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSpanRollupAdjuster(Registry Flags, string Flag, double Percent) : ISpanRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameSpanRollupAdjuster(Dictionary<string, double> Factors) : ISpanRollupStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SpanRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSpanRollupAdjuster(List<double> Bounds, List<double> Rates) : ISpanRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanRollup item, double valueValue)
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

public sealed record SpanRollupVersionOffsetAdjuster(double Weight) : ISpanRollupStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(SpanRollup item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the span rollup steps in order and keeps a trace of each.</summary>
public sealed class SpanRollupEngine(List<ISpanRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanRollupEngine DefaultSpanRollupEngine(Registry flags)
    {
        return new SpanRollupEngine(
            new List<ISpanRollupStep>
            {
                new PercentSpanRollupAdjuster(-24.9),
                new CapSpanRollupAdjuster(12979.0),
                new FlaggedSpanRollupAdjuster(flags, "span-rollup-fallback", 5.2),
                new NameSpanRollupAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.79, ["west"] = 0.93, ["north"] = 1.08 }),
                new TieredSpanRollupAdjuster(
                    new List<double> { 92.0, 6399.0, 7965.0, 8719.0 },
                    new List<double> { 0.82, 0.82, 1.16, 1.16 }),
                new SpanRollupVersionOffsetAdjuster(0.55),
            },
            new List<string>(),
            3.0,
            11372.9);
    }

    public double Compute(SpanRollup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SpanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
