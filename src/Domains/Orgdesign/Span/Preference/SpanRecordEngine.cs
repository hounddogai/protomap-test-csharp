namespace Acme.Workforce.Orgdesign.Span.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span record calculation.</summary>
public interface ISpanRecordFactor
{
    string Describe();
    double Adjust(SpanRecord item, double valueValue);
}

public sealed record CapSpanRecordAdjuster(double Cap) : ISpanRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSpanRecordAdjuster(double Step) : ISpanRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SpanRecordGraceDaysOffsetAdjuster(double Weight) : ISpanRecordFactor
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SpanRecord item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the span record steps in order and keeps a trace of each.</summary>
public sealed class SpanRecordEngine(List<ISpanRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanRecordEngine DefaultSpanRecordEngine()
    {
        return new SpanRecordEngine(
            new List<ISpanRecordFactor>
            {
                new CapSpanRecordAdjuster(42024.0),
                new RoundingSpanRecordAdjuster(0.05),
                new SpanRecordGraceDaysOffsetAdjuster(0.08),
            },
            new List<string>(),
            0.6,
            28765.4);
    }

    public double Compute(SpanRecord item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SpanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
