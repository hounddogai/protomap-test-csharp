namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted adjustment template calculation.</summary>
public interface IWeightedAdjustmentTemplateAdjuster
{
    string Describe();
    double Adjust(WeightedAdjustmentTemplate item, double valueValue);
}

public sealed record DepartmentWeightedAdjustmentTemplateAdjuster(Dictionary<string, double> Factors) : IWeightedAdjustmentTemplateAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(WeightedAdjustmentTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWeightedAdjustmentTemplateAdjuster(double Step) : IWeightedAdjustmentTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAdjustmentTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedAdjustmentTemplateAdjuster(double Minimum) : IWeightedAdjustmentTemplateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAdjustmentTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedAdjustmentTemplateAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAdjustmentTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAdjustmentTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedAdjustmentTemplateAdjuster(double Cap) : IWeightedAdjustmentTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAdjustmentTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWeightedAdjustmentTemplateAdjuster(List<double> Bounds, List<double> Rates) : IWeightedAdjustmentTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedAdjustmentTemplate item, double valueValue)
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

/// <summary>Runs the weighted adjustment template steps in order and keeps a trace of each.</summary>
public sealed class WeightedAdjustmentTemplateEngine(List<IWeightedAdjustmentTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAdjustmentTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAdjustmentTemplateEngine DefaultWeightedAdjustmentTemplateEngine(Registry flags)
    {
        return new WeightedAdjustmentTemplateEngine(
            new List<IWeightedAdjustmentTemplateAdjuster>
            {
                new DepartmentWeightedAdjustmentTemplateAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.18, ["central"] = 0.84 }),
                new RoundingWeightedAdjustmentTemplateAdjuster(0.1),
                new MinimumWeightedAdjustmentTemplateAdjuster(24.9),
                new FlaggedWeightedAdjustmentTemplateAdjuster(flags, "weighted-adjustment-template-adaptive", 9.7),
                new CapWeightedAdjustmentTemplateAdjuster(46251.0),
                new TieredWeightedAdjustmentTemplateAdjuster(
                    new List<double> { 6485.0, 7254.0, 7627.0, 8025.0 },
                    new List<double> { 0.97, 1.07, 1.2, 1.16 }),
            },
            new List<string>(),
            2.6,
            89028.3);
    }

    public double Price(WeightedAdjustmentTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAdjustmentTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
