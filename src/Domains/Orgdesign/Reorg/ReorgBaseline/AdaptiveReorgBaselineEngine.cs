namespace Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive reorg baseline calculation.</summary>
public interface IAdaptiveReorgBaselineAdjuster
{
    string Describe();
    double Adjust(AdaptiveReorgBaseline item, double valueValue);
}

public sealed record RoundingAdaptiveReorgBaselineAdjuster(double Step) : IAdaptiveReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveReorgBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAdaptiveReorgBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveReorgBaseline item, double valueValue)
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

public sealed record AdaptiveReorgBaselineRetriesOffsetAdjuster(double Weight) : IAdaptiveReorgBaselineAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AdaptiveReorgBaseline item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the adaptive reorg baseline steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveReorgBaselineEngine(List<IAdaptiveReorgBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveReorgBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveReorgBaselineEngine DefaultAdaptiveReorgBaselineEngine()
    {
        return new AdaptiveReorgBaselineEngine(
            new List<IAdaptiveReorgBaselineAdjuster>
            {
                new RoundingAdaptiveReorgBaselineAdjuster(0.1),
                new TieredAdaptiveReorgBaselineAdjuster(
                    new List<double> { 1080.0, 4386.0, 7497.0, 9617.0 },
                    new List<double> { 1.1, 0.95, 0.88, 1.18 }),
                new AdaptiveReorgBaselineRetriesOffsetAdjuster(1.91),
            },
            new List<string>(),
            0.3,
            95253.4);
    }

    public double Evaluate(AdaptiveReorgBaseline item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveReorgBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
