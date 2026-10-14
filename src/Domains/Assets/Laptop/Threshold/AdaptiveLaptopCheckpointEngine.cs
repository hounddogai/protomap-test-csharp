namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive laptop checkpoint calculation.</summary>
public interface IAdaptiveLaptopCheckpointFactor
{
    string Describe();
    double Adjust(AdaptiveLaptopCheckpoint item, double valueValue);
}

public sealed record PercentAdaptiveLaptopCheckpointAdjuster(double Percent) : IAdaptiveLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveLaptopCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptiveLaptopCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveLaptopCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveLaptopCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveLaptopCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveLaptopCheckpoint item, double valueValue)
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

/// <summary>Runs the adaptive laptop checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveLaptopCheckpointEngine(List<IAdaptiveLaptopCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveLaptopCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveLaptopCheckpointEngine DefaultAdaptiveLaptopCheckpointEngine(Registry flags)
    {
        return new AdaptiveLaptopCheckpointEngine(
            new List<IAdaptiveLaptopCheckpointFactor>
            {
                new PercentAdaptiveLaptopCheckpointAdjuster(22.0),
                new FlaggedAdaptiveLaptopCheckpointAdjuster(flags, "adaptive-laptop-checkpoint-scheduled", 2.1),
                new TieredAdaptiveLaptopCheckpointAdjuster(
                    new List<double> { 1116.0, 2197.0, 2230.0, 9200.0 },
                    new List<double> { 0.96, 1.07, 1.0, 0.86 }),
            },
            new List<string>(),
            2.9,
            78824.9);
    }

    public double Evaluate(AdaptiveLaptopCheckpoint item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveLaptopCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
