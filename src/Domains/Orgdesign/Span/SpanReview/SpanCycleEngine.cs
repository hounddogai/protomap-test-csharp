namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span cycle calculation.</summary>
public interface ISpanCycleFactor
{
    string Describe();
    double Adjust(SpanCycle item, double valueValue);
}

public sealed record FlaggedSpanCycleAdjuster(Registry Flags, string Flag, double Percent) : ISpanCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSpanCycleAdjuster(List<double> Bounds, List<double> Rates) : ISpanCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanCycle item, double valueValue)
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

public sealed record PercentSpanCycleAdjuster(double Percent) : ISpanCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentSpanCycleAdjuster(Dictionary<string, double> Factors) : ISpanCycleFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSpanCycleAdjuster(double Minimum) : ISpanCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSpanCycleAdjuster(double Cap) : ISpanCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span cycle steps in order and keeps a trace of each.</summary>
public sealed class SpanCycleEngine(List<ISpanCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanCycleEngine DefaultSpanCycleEngine(Registry flags)
    {
        return new SpanCycleEngine(
            new List<ISpanCycleFactor>
            {
                new FlaggedSpanCycleAdjuster(flags, "span-cycle-batched", 7.5),
                new TieredSpanCycleAdjuster(
                    new List<double> { 5271.0, 5930.0, 8993.0, 9907.0 },
                    new List<double> { 0.9, 1.03, 0.91, 1.01 }),
                new PercentSpanCycleAdjuster(-3.0),
                new DepartmentSpanCycleAdjuster(new Dictionary<string, double> { ["retail"] = 1.15 }),
                new MinimumSpanCycleAdjuster(26.4),
                new CapSpanCycleAdjuster(15710.0),
            },
            new List<string>(),
            3.5,
            94329.5);
    }

    public double Run(SpanCycle item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SpanCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
