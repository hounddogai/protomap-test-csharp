namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered laptop baseline calculation.</summary>
public interface ITieredLaptopBaselineStep
{
    string Describe();
    double Adjust(TieredLaptopBaseline item, double valueValue);
}

public sealed record FlaggedTieredLaptopBaselineAdjuster(Registry Flags, string Flag, double Percent) : ITieredLaptopBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredLaptopBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitTieredLaptopBaselineAdjuster(Dictionary<string, double> Factors) : ITieredLaptopBaselineStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(TieredLaptopBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTieredLaptopBaselineAdjuster(double Minimum) : ITieredLaptopBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredLaptopBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTieredLaptopBaselineAdjuster(double Step) : ITieredLaptopBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredLaptopBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTieredLaptopBaselineAdjuster(List<double> Bounds, List<double> Rates) : ITieredLaptopBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredLaptopBaseline item, double valueValue)
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

/// <summary>Runs the tiered laptop baseline steps in order and keeps a trace of each.</summary>
public sealed class TieredLaptopBaselineEngine(List<ITieredLaptopBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLaptopBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLaptopBaselineEngine DefaultTieredLaptopBaselineEngine(Registry flags)
    {
        return new TieredLaptopBaselineEngine(
            new List<ITieredLaptopBaselineStep>
            {
                new FlaggedTieredLaptopBaselineAdjuster(flags, "tiered-laptop-baseline-legacy", -3.0),
                new UnitTieredLaptopBaselineAdjuster(new Dictionary<string, double> { ["retail"] = 0.98 }),
                new MinimumTieredLaptopBaselineAdjuster(7.4),
                new RoundingTieredLaptopBaselineAdjuster(0.25),
                new TieredTieredLaptopBaselineAdjuster(
                    new List<double> { 1067.0, 3951.0, 3959.0, 9189.0 },
                    new List<double> { 1.19, 0.93, 1.14, 0.84 }),
            },
            new List<string>(),
            1.4,
            78463.6);
    }

    public double Price(TieredLaptopBaseline item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLaptopBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
