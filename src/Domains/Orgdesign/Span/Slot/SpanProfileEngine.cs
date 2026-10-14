namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span profile calculation.</summary>
public interface ISpanProfileAdjuster
{
    string Describe();
    double Adjust(SpanProfile item, double valueValue);
}

public sealed record PercentSpanProfileAdjuster(double Percent) : ISpanProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSpanProfileAdjuster(double Minimum) : ISpanProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSpanProfileAdjuster(double Step) : ISpanProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSpanProfileAdjuster(double Cap) : ISpanProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span profile steps in order and keeps a trace of each.</summary>
public sealed class SpanProfileEngine(List<ISpanProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanProfileEngine DefaultSpanProfileEngine()
    {
        return new SpanProfileEngine(
            new List<ISpanProfileAdjuster>
            {
                new PercentSpanProfileAdjuster(17.3),
                new MinimumSpanProfileAdjuster(14.0),
                new RoundingSpanProfileAdjuster(0.5),
                new CapSpanProfileAdjuster(42327.0),
            },
            new List<string>(),
            4.5,
            7411.1);
    }

    public double Price(SpanProfile item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SpanProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
