namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted balance threshold calculation.</summary>
public interface IWeightedBalanceThresholdStep
{
    string Describe();
    double Adjust(WeightedBalanceThreshold item, double valueValue);
}

public sealed record TieredWeightedBalanceThresholdAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBalanceThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBalanceThreshold item, double valueValue)
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

public sealed record PercentWeightedBalanceThresholdAdjuster(double Percent) : IWeightedBalanceThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedBalanceThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWeightedBalanceThresholdAdjuster(Registry Flags, string Flag, double Percent) : IWeightedBalanceThresholdStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedBalanceThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted balance threshold steps in order and keeps a trace of each.</summary>
public sealed class WeightedBalanceThresholdEngine(List<IWeightedBalanceThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBalanceThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBalanceThresholdEngine DefaultWeightedBalanceThresholdEngine(Registry flags)
    {
        return new WeightedBalanceThresholdEngine(
            new List<IWeightedBalanceThresholdStep>
            {
                new TieredWeightedBalanceThresholdAdjuster(
                    new List<double> { 961.0, 2118.0, 3287.0 },
                    new List<double> { 0.87, 0.94, 1.0 }),
                new PercentWeightedBalanceThresholdAdjuster(-10.7),
                new FlaggedWeightedBalanceThresholdAdjuster(flags, "weighted-balance-threshold-manual", -8.4),
            },
            new List<string>(),
            2.4,
            91242.1);
    }

    public double Compute(WeightedBalanceThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBalanceThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
