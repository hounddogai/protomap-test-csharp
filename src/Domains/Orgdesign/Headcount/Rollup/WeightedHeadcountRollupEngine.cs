namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted headcount rollup calculation.</summary>
public interface IWeightedHeadcountRollupAdjuster
{
    string Describe();
    double Adjust(WeightedHeadcountRollup item, double valueValue);
}

public sealed record WeightedHeadcountRollupQuantityOffsetAdjuster(double Weight) : IWeightedHeadcountRollupAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(WeightedHeadcountRollup item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record LabelWeightedHeadcountRollupAdjuster(Dictionary<string, double> Factors) : IWeightedHeadcountRollupAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedHeadcountRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedHeadcountRollupAdjuster(double Minimum) : IWeightedHeadcountRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedHeadcountRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedHeadcountRollupAdjuster(Registry Flags, string Flag, double Percent) : IWeightedHeadcountRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedHeadcountRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedHeadcountRollupAdjuster(double Cap) : IWeightedHeadcountRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedHeadcountRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedHeadcountRollupAdjuster(double Percent) : IWeightedHeadcountRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedHeadcountRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted headcount rollup steps in order and keeps a trace of each.</summary>
public sealed class WeightedHeadcountRollupEngine(List<IWeightedHeadcountRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedHeadcountRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedHeadcountRollupEngine DefaultWeightedHeadcountRollupEngine(Registry flags)
    {
        return new WeightedHeadcountRollupEngine(
            new List<IWeightedHeadcountRollupAdjuster>
            {
                new WeightedHeadcountRollupQuantityOffsetAdjuster(1.05),
                new LabelWeightedHeadcountRollupAdjuster(new Dictionary<string, double> { ["west"] = 0.78 }),
                new MinimumWeightedHeadcountRollupAdjuster(34.9),
                new FlaggedWeightedHeadcountRollupAdjuster(flags, "weighted-headcount-rollup-manual", 5.7),
                new CapWeightedHeadcountRollupAdjuster(2902.0),
                new PercentWeightedHeadcountRollupAdjuster(12.1),
            },
            new List<string>(),
            2.0,
            70067.2);
    }

    public double Price(WeightedHeadcountRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedHeadcountRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
