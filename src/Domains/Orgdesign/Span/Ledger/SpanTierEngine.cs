namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span tier calculation.</summary>
public interface ISpanTierAdjuster
{
    string Describe();
    double Adjust(SpanTier item, double valueValue);
}

public sealed record TieredSpanTierAdjuster(List<double> Bounds, List<double> Rates) : ISpanTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanTier item, double valueValue)
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

public sealed record MinimumSpanTierAdjuster(double Minimum) : ISpanTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSpanTierAdjuster(double Step) : ISpanTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSpanTierAdjuster(Registry Flags, string Flag, double Percent) : ISpanTierAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentSpanTierAdjuster(Dictionary<string, double> Factors) : ISpanTierAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the span tier steps in order and keeps a trace of each.</summary>
public sealed class SpanTierEngine(List<ISpanTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanTierEngine DefaultSpanTierEngine(Registry flags)
    {
        return new SpanTierEngine(
            new List<ISpanTierAdjuster>
            {
                new TieredSpanTierAdjuster(
                    new List<double> { 1911.0, 2638.0, 4858.0 },
                    new List<double> { 0.87, 0.88, 0.97 }),
                new MinimumSpanTierAdjuster(45.6),
                new RoundingSpanTierAdjuster(0.25),
                new FlaggedSpanTierAdjuster(flags, "span-tier-pending", 2.9),
                new DepartmentSpanTierAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.0, ["south"] = 1.07 }),
            },
            new List<string>(),
            1.6,
            24736.9);
    }

    public double Compute(SpanTier item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SpanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
