namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span summary calculation.</summary>
public interface ISpanSummaryAdjuster
{
    string Describe();
    double Adjust(SpanSummary item, double valueValue);
}

public sealed record TieredSpanSummaryAdjuster(List<double> Bounds, List<double> Rates) : ISpanSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanSummary item, double valueValue)
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

public sealed record LabelSpanSummaryAdjuster(Dictionary<string, double> Factors) : ISpanSummaryAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SpanSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSpanSummaryAdjuster(double Cap) : ISpanSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSpanSummaryAdjuster(double Minimum) : ISpanSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSpanSummaryAdjuster(double Step) : ISpanSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the span summary steps in order and keeps a trace of each.</summary>
public sealed class SpanSummaryEngine(List<ISpanSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanSummaryEngine DefaultSpanSummaryEngine()
    {
        return new SpanSummaryEngine(
            new List<ISpanSummaryAdjuster>
            {
                new TieredSpanSummaryAdjuster(
                    new List<double> { 1289.0, 2143.0, 5567.0 },
                    new List<double> { 1.15, 1.01, 0.92 }),
                new LabelSpanSummaryAdjuster(new Dictionary<string, double> { ["retail"] = 1.24 }),
                new CapSpanSummaryAdjuster(1200.0),
                new MinimumSpanSummaryAdjuster(9.8),
                new RoundingSpanSummaryAdjuster(0.25),
            },
            new List<string>(),
            1.2,
            11110.3);
    }

    public double Evaluate(SpanSummary item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SpanSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
