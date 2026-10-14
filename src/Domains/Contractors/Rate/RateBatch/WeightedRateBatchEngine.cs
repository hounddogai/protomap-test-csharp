namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted rate batch calculation.</summary>
public interface IWeightedRateBatchModifier
{
    string Describe();
    double Adjust(WeightedRateBatch item, double valueValue);
}

public sealed record RoundingWeightedRateBatchAdjuster(double Step) : IWeightedRateBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedRateBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedRateBatchAdjuster(double Minimum) : IWeightedRateBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedRateBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedRateBatchAdjuster(Registry Flags, string Flag, double Percent) : IWeightedRateBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedRateBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted rate batch steps in order and keeps a trace of each.</summary>
public sealed class WeightedRateBatchEngine(List<IWeightedRateBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedRateBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedRateBatchEngine DefaultWeightedRateBatchEngine(Registry flags)
    {
        return new WeightedRateBatchEngine(
            new List<IWeightedRateBatchModifier>
            {
                new RoundingWeightedRateBatchAdjuster(0.5),
                new MinimumWeightedRateBatchAdjuster(45.0),
                new FlaggedWeightedRateBatchAdjuster(flags, "weighted-rate-batch-manual", -0.7),
            },
            new List<string>(),
            1.7,
            60377.6);
    }

    public double Run(WeightedRateBatch item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedRateBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
