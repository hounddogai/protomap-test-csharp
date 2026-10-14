namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span adjustment calculation.</summary>
public interface ISpanAdjustmentStep
{
    string Describe();
    double Adjust(SpanAdjustment item, double valueValue);
}

public sealed record CapSpanAdjustmentAdjuster(double Cap) : ISpanAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSpanAdjustmentAdjuster(double Step) : ISpanAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSpanAdjustmentAdjuster(double Percent) : ISpanAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleSpanAdjustmentAdjuster(Dictionary<string, double> Factors) : ISpanAdjustmentStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SpanAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the span adjustment steps in order and keeps a trace of each.</summary>
public sealed class SpanAdjustmentEngine(List<ISpanAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanAdjustmentEngine DefaultSpanAdjustmentEngine()
    {
        return new SpanAdjustmentEngine(
            new List<ISpanAdjustmentStep>
            {
                new CapSpanAdjustmentAdjuster(8526.0),
                new RoundingSpanAdjustmentAdjuster(0.5),
                new PercentSpanAdjustmentAdjuster(-20.6),
                new LocaleSpanAdjustmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.92 }),
            },
            new List<string>(),
            3.3,
            2401.6);
    }

    public double Evaluate(SpanAdjustment item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SpanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
