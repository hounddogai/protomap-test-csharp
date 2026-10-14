namespace Acme.Workforce.Orgdesign.Span.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span baseline calculation.</summary>
public interface ISpanBaselineStep
{
    string Describe();
    double Adjust(SpanBaseline item, double valueValue);
}

public sealed record RoundingSpanBaselineAdjuster(double Step) : ISpanBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameSpanBaselineAdjuster(Dictionary<string, double> Factors) : ISpanBaselineStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SpanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSpanBaselineAdjuster(double Cap) : ISpanBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span baseline steps in order and keeps a trace of each.</summary>
public sealed class SpanBaselineEngine(List<ISpanBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanBaselineEngine DefaultSpanBaselineEngine()
    {
        return new SpanBaselineEngine(
            new List<ISpanBaselineStep>
            {
                new RoundingSpanBaselineAdjuster(0.5),
                new NameSpanBaselineAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.16, ["west"] = 0.94, ["central"] = 1.08 }),
                new CapSpanBaselineAdjuster(13192.0),
            },
            new List<string>(),
            4.6,
            47027.8);
    }

    public double Evaluate(SpanBaseline item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SpanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
