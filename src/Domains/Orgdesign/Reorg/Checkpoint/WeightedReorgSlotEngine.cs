namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted reorg slot calculation.</summary>
public interface IWeightedReorgSlotFactor
{
    string Describe();
    double Adjust(WeightedReorgSlot item, double valueValue);
}

public sealed record RoundingWeightedReorgSlotAdjuster(double Step) : IWeightedReorgSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedReorgSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWeightedReorgSlotAdjuster(List<double> Bounds, List<double> Rates) : IWeightedReorgSlotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedReorgSlot item, double valueValue)
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

public sealed record PercentWeightedReorgSlotAdjuster(double Percent) : IWeightedReorgSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedReorgSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted reorg slot steps in order and keeps a trace of each.</summary>
public sealed class WeightedReorgSlotEngine(List<IWeightedReorgSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedReorgSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedReorgSlotEngine DefaultWeightedReorgSlotEngine()
    {
        return new WeightedReorgSlotEngine(
            new List<IWeightedReorgSlotFactor>
            {
                new RoundingWeightedReorgSlotAdjuster(0.1),
                new TieredWeightedReorgSlotAdjuster(
                    new List<double> { 486.0, 1452.0, 1953.0, 7397.0 },
                    new List<double> { 1.12, 1.05, 1.02, 1.17 }),
                new PercentWeightedReorgSlotAdjuster(-23.2),
            },
            new List<string>(),
            0.9,
            58077.9);
    }

    public double Evaluate(WeightedReorgSlot item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedReorgSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
