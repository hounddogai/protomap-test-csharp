namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span estimate calculation.</summary>
public interface ISpanEstimateModifier
{
    string Describe();
    double Adjust(SpanEstimate item, double valueValue);
}

public sealed record SpanEstimateUpdatedAtOffsetAdjuster(double Weight) : ISpanEstimateModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(SpanEstimate item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentSpanEstimateAdjuster(double Percent) : ISpanEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSpanEstimateAdjuster(double Step) : ISpanEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentSpanEstimateAdjuster(Dictionary<string, double> Factors) : ISpanEstimateModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the span estimate steps in order and keeps a trace of each.</summary>
public sealed class SpanEstimateEngine(List<ISpanEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanEstimateEngine DefaultSpanEstimateEngine()
    {
        return new SpanEstimateEngine(
            new List<ISpanEstimateModifier>
            {
                new SpanEstimateUpdatedAtOffsetAdjuster(0.48),
                new PercentSpanEstimateAdjuster(3.9),
                new RoundingSpanEstimateAdjuster(0.5),
                new DepartmentSpanEstimateAdjuster(new Dictionary<string, double> { ["online"] = 0.98 }),
            },
            new List<string>(),
            4.5,
            55180.7);
    }

    public double Run(SpanEstimate item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SpanEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
