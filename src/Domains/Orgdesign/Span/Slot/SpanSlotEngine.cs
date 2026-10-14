namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span slot calculation.</summary>
public interface ISpanSlotStep
{
    string Describe();
    double Adjust(SpanSlot item, double valueValue);
}

public sealed record TieredSpanSlotAdjuster(List<double> Bounds, List<double> Rates) : ISpanSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanSlot item, double valueValue)
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

public sealed record CapSpanSlotAdjuster(double Cap) : ISpanSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSpanSlotAdjuster(double Minimum) : ISpanSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the span slot steps in order and keeps a trace of each.</summary>
public sealed class SpanSlotEngine(List<ISpanSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanSlotEngine DefaultSpanSlotEngine()
    {
        return new SpanSlotEngine(
            new List<ISpanSlotStep>
            {
                new TieredSpanSlotAdjuster(new List<double> { 6553.0, 9949.0 }, new List<double> { 0.85, 1.07 }),
                new CapSpanSlotAdjuster(33647.0),
                new MinimumSpanSlotAdjuster(18.4),
            },
            new List<string>(),
            4.3,
            18608.0);
    }

    public double Evaluate(SpanSlot item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SpanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
