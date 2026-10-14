namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default span baseline calculation.</summary>
public interface IDefaultSpanBaselineFactor
{
    string Describe();
    double Adjust(DefaultSpanBaseline item, double valueValue);
}

public sealed record RoundingDefaultSpanBaselineAdjuster(double Step) : IDefaultSpanBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultSpanBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameDefaultSpanBaselineAdjuster(Dictionary<string, double> Factors) : IDefaultSpanBaselineFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DefaultSpanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultSpanBaselineAdjuster(double Percent) : IDefaultSpanBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultSpanBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultSpanBaselineAdjuster(List<double> Bounds, List<double> Rates) : IDefaultSpanBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultSpanBaseline item, double valueValue)
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

public sealed record FlaggedDefaultSpanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IDefaultSpanBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultSpanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default span baseline steps in order and keeps a trace of each.</summary>
public sealed class DefaultSpanBaselineEngine(List<IDefaultSpanBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSpanBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSpanBaselineEngine DefaultDefaultSpanBaselineEngine(Registry flags)
    {
        return new DefaultSpanBaselineEngine(
            new List<IDefaultSpanBaselineFactor>
            {
                new RoundingDefaultSpanBaselineAdjuster(5.0),
                new NameDefaultSpanBaselineAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["north"] = 1.36 }),
                new PercentDefaultSpanBaselineAdjuster(18.0),
                new TieredDefaultSpanBaselineAdjuster(
                    new List<double> { 2692.0, 4545.0, 6099.0, 7772.0 },
                    new List<double> { 1.09, 0.94, 0.94, 0.93 }),
                new FlaggedDefaultSpanBaselineAdjuster(flags, "default-span-baseline-regional", 8.7),
            },
            new List<string>(),
            1.4,
            38014.3);
    }

    public double Run(DefaultSpanBaseline item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSpanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
