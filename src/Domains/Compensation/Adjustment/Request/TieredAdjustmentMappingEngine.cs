namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered adjustment mapping calculation.</summary>
public interface ITieredAdjustmentMappingFactor
{
    string Describe();
    double Adjust(TieredAdjustmentMapping item, double valueValue);
}

public sealed record MinimumTieredAdjustmentMappingAdjuster(double Minimum) : ITieredAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredAdjustmentMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTieredAdjustmentMappingAdjuster(Registry Flags, string Flag, double Percent) : ITieredAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredAdjustmentMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdjustmentMappingBatchSizeOffsetAdjuster(double Weight) : ITieredAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(TieredAdjustmentMapping item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentTieredAdjustmentMappingAdjuster(double Percent) : ITieredAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredAdjustmentMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameTieredAdjustmentMappingAdjuster(Dictionary<string, double> Factors) : ITieredAdjustmentMappingFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredAdjustmentMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredAdjustmentMappingAdjuster(List<double> Bounds, List<double> Rates) : ITieredAdjustmentMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredAdjustmentMapping item, double valueValue)
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

/// <summary>Runs the tiered adjustment mapping steps in order and keeps a trace of each.</summary>
public sealed class TieredAdjustmentMappingEngine(List<ITieredAdjustmentMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredAdjustmentMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredAdjustmentMappingEngine DefaultTieredAdjustmentMappingEngine(Registry flags)
    {
        return new TieredAdjustmentMappingEngine(
            new List<ITieredAdjustmentMappingFactor>
            {
                new MinimumTieredAdjustmentMappingAdjuster(30.0),
                new FlaggedTieredAdjustmentMappingAdjuster(flags, "tiered-adjustment-mapping-lenient", 6.4),
                new TieredAdjustmentMappingBatchSizeOffsetAdjuster(1.98),
                new PercentTieredAdjustmentMappingAdjuster(-9.9),
                new NameTieredAdjustmentMappingAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.36, ["south"] = 0.98, ["north"] = 0.73 }),
                new TieredTieredAdjustmentMappingAdjuster(
                    new List<double> { 4814.0, 5525.0, 9277.0 },
                    new List<double> { 1.16, 0.95, 0.93 }),
            },
            new List<string>(),
            2.7,
            66263.9);
    }

    public double Run(TieredAdjustmentMapping item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredAdjustmentMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
