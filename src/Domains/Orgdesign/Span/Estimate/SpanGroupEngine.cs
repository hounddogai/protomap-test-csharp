namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the span group calculation.</summary>
public interface ISpanGroupAdjuster
{
    string Describe();
    double Adjust(SpanGroup item, double valueValue);
}

public sealed record PercentSpanGroupAdjuster(double Percent) : ISpanGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSpanGroupAdjuster(List<double> Bounds, List<double> Rates) : ISpanGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanGroup item, double valueValue)
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

public sealed record DepartmentSpanGroupAdjuster(Dictionary<string, double> Factors) : ISpanGroupAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSpanGroupAdjuster(Registry Flags, string Flag, double Percent) : ISpanGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the span group steps in order and keeps a trace of each.</summary>
public sealed class SpanGroupEngine(List<ISpanGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanGroupEngine DefaultSpanGroupEngine(Registry flags)
    {
        return new SpanGroupEngine(
            new List<ISpanGroupAdjuster>
            {
                new PercentSpanGroupAdjuster(24.3),
                new TieredSpanGroupAdjuster(
                    new List<double> { 2369.0, 5449.0, 9149.0, 9341.0 },
                    new List<double> { 1.07, 0.9, 1.06, 1.05 }),
                new DepartmentSpanGroupAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.75, ["retail"] = 0.88, ["south"] = 1.4 }),
                new FlaggedSpanGroupAdjuster(flags, "span-group-capped", 4.7),
            },
            new List<string>(),
            3.1,
            26765.5);
    }

    public double Compute(SpanGroup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SpanGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
