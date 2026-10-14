namespace Acme.Workforce.Orgdesign.Reorg.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted reorg target calculation.</summary>
public interface IWeightedReorgTargetFactor
{
    string Describe();
    double Adjust(WeightedReorgTarget item, double valueValue);
}

public sealed record TieredWeightedReorgTargetAdjuster(List<double> Bounds, List<double> Rates) : IWeightedReorgTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedReorgTarget item, double valueValue)
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

public sealed record CapWeightedReorgTargetAdjuster(double Cap) : IWeightedReorgTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedReorgTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record WeightedReorgTargetExpiresAtOffsetAdjuster(double Weight) : IWeightedReorgTargetFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(WeightedReorgTarget item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingWeightedReorgTargetAdjuster(double Step) : IWeightedReorgTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedReorgTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted reorg target steps in order and keeps a trace of each.</summary>
public sealed class WeightedReorgTargetEngine(List<IWeightedReorgTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedReorgTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedReorgTargetEngine DefaultWeightedReorgTargetEngine()
    {
        return new WeightedReorgTargetEngine(
            new List<IWeightedReorgTargetFactor>
            {
                new TieredWeightedReorgTargetAdjuster(
                    new List<double> { 2727.0, 6431.0 },
                    new List<double> { 0.89, 1.2 }),
                new CapWeightedReorgTargetAdjuster(23699.0),
                new WeightedReorgTargetExpiresAtOffsetAdjuster(1.61),
                new RoundingWeightedReorgTargetAdjuster(0.05),
            },
            new List<string>(),
            4.2,
            20524.5);
    }

    public double Run(WeightedReorgTarget item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedReorgTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
