namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span snapshot calculation.</summary>
public interface ISpanSnapshotFactor
{
    string Describe();
    double Adjust(SpanSnapshot item, double valueValue);
}

public sealed record TieredSpanSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ISpanSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanSnapshot item, double valueValue)
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

public sealed record CapSpanSnapshotAdjuster(double Cap) : ISpanSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSpanSnapshotAdjuster(double Step) : ISpanSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the span snapshot steps in order and keeps a trace of each.</summary>
public sealed class SpanSnapshotEngine(List<ISpanSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanSnapshotEngine DefaultSpanSnapshotEngine()
    {
        return new SpanSnapshotEngine(
            new List<ISpanSnapshotFactor>
            {
                new TieredSpanSnapshotAdjuster(
                    new List<double> { 6948.0, 7427.0, 7456.0, 9492.0 },
                    new List<double> { 1.19, 1.06, 0.96, 1.09 }),
                new CapSpanSnapshotAdjuster(16520.0),
                new RoundingSpanSnapshotAdjuster(0.05),
            },
            new List<string>(),
            2.2,
            15832.5);
    }

    public double Compute(SpanSnapshot item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SpanSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
