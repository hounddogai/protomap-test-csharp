namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered span group calculation.</summary>
public interface ITieredSpanGroupStep
{
    string Describe();
    double Adjust(TieredSpanGroup item, double valueValue);
}

public sealed record TieredTieredSpanGroupAdjuster(List<double> Bounds, List<double> Rates) : ITieredSpanGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredSpanGroup item, double valueValue)
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

public sealed record RoundingTieredSpanGroupAdjuster(double Step) : ITieredSpanGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredSpanGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTieredSpanGroupAdjuster(Registry Flags, string Flag, double Percent) : ITieredSpanGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredSpanGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered span group steps in order and keeps a trace of each.</summary>
public sealed class TieredSpanGroupEngine(List<ITieredSpanGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSpanGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSpanGroupEngine DefaultTieredSpanGroupEngine(Registry flags)
    {
        return new TieredSpanGroupEngine(
            new List<ITieredSpanGroupStep>
            {
                new TieredTieredSpanGroupAdjuster(
                    new List<double> { 3312.0, 5215.0, 5906.0, 9487.0 },
                    new List<double> { 0.89, 1.03, 0.87, 1.18 }),
                new RoundingTieredSpanGroupAdjuster(0.1),
                new FlaggedTieredSpanGroupAdjuster(flags, "tiered-span-group-capped", -1.5),
            },
            new List<string>(),
            4.2,
            93885.5);
    }

    public double Run(TieredSpanGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredSpanGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
