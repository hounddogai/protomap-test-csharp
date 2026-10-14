namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive cost model checkpoint calculation.</summary>
public interface IAdaptiveCostModelCheckpointStep
{
    string Describe();
    double Adjust(AdaptiveCostModelCheckpoint item, double valueValue);
}

public sealed record RoundingAdaptiveCostModelCheckpointAdjuster(double Step) : IAdaptiveCostModelCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveCostModelCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAdaptiveCostModelCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveCostModelCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveCostModelCheckpoint item, double valueValue)
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

public sealed record MinimumAdaptiveCostModelCheckpointAdjuster(double Minimum) : IAdaptiveCostModelCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveCostModelCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdaptiveCostModelCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveCostModelCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveCostModelCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveCostModelCheckpointGraceDaysOffsetAdjuster(double Weight) : IAdaptiveCostModelCheckpointStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(AdaptiveCostModelCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the adaptive cost model checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCostModelCheckpointEngine(List<IAdaptiveCostModelCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCostModelCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCostModelCheckpointEngine DefaultAdaptiveCostModelCheckpointEngine(Registry flags)
    {
        return new AdaptiveCostModelCheckpointEngine(
            new List<IAdaptiveCostModelCheckpointStep>
            {
                new RoundingAdaptiveCostModelCheckpointAdjuster(5.0),
                new TieredAdaptiveCostModelCheckpointAdjuster(
                    new List<double> { 3328.0, 3617.0, 4631.0, 6984.0 },
                    new List<double> { 0.97, 1.06, 0.88, 1.15 }),
                new MinimumAdaptiveCostModelCheckpointAdjuster(46.4),
                new FlaggedAdaptiveCostModelCheckpointAdjuster(flags, "adaptive-cost-model-checkpoint-tiered", -9.0),
                new AdaptiveCostModelCheckpointGraceDaysOffsetAdjuster(0.74),
            },
            new List<string>(),
            0.8,
            67062.0);
    }

    public double Compute(AdaptiveCostModelCheckpoint item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCostModelCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
