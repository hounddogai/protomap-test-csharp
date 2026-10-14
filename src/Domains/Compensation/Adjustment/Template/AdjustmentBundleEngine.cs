namespace Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>One step of the adjustment bundle calculation.</summary>
public interface IAdjustmentBundleModifier
{
    string Describe();
    double Adjust(AdjustmentBundle item, double valueValue);
}

public sealed record TieredAdjustmentBundleAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentBundleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentBundle item, double valueValue)
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

public sealed record TitleAdjustmentBundleAdjuster(Dictionary<string, double> Factors) : IAdjustmentBundleModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AdjustmentBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAdjustmentBundleAdjuster(double Minimum) : IAdjustmentBundleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdjustmentBundleMinimumOffsetAdjuster(double Weight) : IAdjustmentBundleModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AdjustmentBundle item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the adjustment bundle steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentBundleEngine(List<IAdjustmentBundleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentBundleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentBundleEngine DefaultAdjustmentBundleEngine()
    {
        return new AdjustmentBundleEngine(
            new List<IAdjustmentBundleModifier>
            {
                new TieredAdjustmentBundleAdjuster(
                    new List<double> { 2140.0, 4041.0 },
                    new List<double> { 0.94, 1.08 }),
                new TitleAdjustmentBundleAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.0, ["retail"] = 1.21, ["east"] = 1.26 }),
                new MinimumAdjustmentBundleAdjuster(27.7),
                new AdjustmentBundleMinimumOffsetAdjuster(0.89),
            },
            new List<string>(),
            3.2,
            74172.5);
    }

    public double Price(AdjustmentBundle item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
