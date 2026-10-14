namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted span baseline calculation.</summary>
public interface IWeightedSpanBaselineFactor
{
    string Describe();
    double Adjust(WeightedSpanBaseline item, double valueValue);
}

public sealed record TitleWeightedSpanBaselineAdjuster(Dictionary<string, double> Factors) : IWeightedSpanBaselineFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WeightedSpanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredWeightedSpanBaselineAdjuster(List<double> Bounds, List<double> Rates) : IWeightedSpanBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedSpanBaseline item, double valueValue)
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

public sealed record MinimumWeightedSpanBaselineAdjuster(double Minimum) : IWeightedSpanBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedSpanBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WeightedSpanBaselineEndsAtOffsetAdjuster(double Weight) : IWeightedSpanBaselineFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(WeightedSpanBaseline item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record FlaggedWeightedSpanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IWeightedSpanBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedSpanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted span baseline steps in order and keeps a trace of each.</summary>
public sealed class WeightedSpanBaselineEngine(List<IWeightedSpanBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedSpanBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedSpanBaselineEngine DefaultWeightedSpanBaselineEngine(Registry flags)
    {
        return new WeightedSpanBaselineEngine(
            new List<IWeightedSpanBaselineFactor>
            {
                new TitleWeightedSpanBaselineAdjuster(new Dictionary<string, double> { ["retail"] = 1.0 }),
                new TieredWeightedSpanBaselineAdjuster(
                    new List<double> { 2132.0, 6357.0, 9936.0 },
                    new List<double> { 0.8, 0.86, 0.92 }),
                new MinimumWeightedSpanBaselineAdjuster(44.8),
                new WeightedSpanBaselineEndsAtOffsetAdjuster(1.72),
                new FlaggedWeightedSpanBaselineAdjuster(flags, "weighted-span-baseline-weighted", -1.3),
            },
            new List<string>(),
            4.1,
            62864.3);
    }

    public double Evaluate(WeightedSpanBaseline item)
    {
        var current = item.AdjustedExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<WeightedSpanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
