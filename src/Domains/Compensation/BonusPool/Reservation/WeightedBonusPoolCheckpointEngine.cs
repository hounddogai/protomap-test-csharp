namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted bonus pool checkpoint calculation.</summary>
public interface IWeightedBonusPoolCheckpointStep
{
    string Describe();
    double Adjust(WeightedBonusPoolCheckpoint item, double valueValue);
}

public sealed record TieredWeightedBonusPoolCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBonusPoolCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBonusPoolCheckpoint item, double valueValue)
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

public sealed record MinimumWeightedBonusPoolCheckpointAdjuster(double Minimum) : IWeightedBonusPoolCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedBonusPoolCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedBonusPoolCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IWeightedBonusPoolCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedBonusPoolCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedBonusPoolCheckpointWindowDaysOffsetAdjuster(double Weight) : IWeightedBonusPoolCheckpointStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(WeightedBonusPoolCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the weighted bonus pool checkpoint steps in order and keeps a trace of each.</summary>
public sealed class WeightedBonusPoolCheckpointEngine(List<IWeightedBonusPoolCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBonusPoolCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBonusPoolCheckpointEngine DefaultWeightedBonusPoolCheckpointEngine(Registry flags)
    {
        return new WeightedBonusPoolCheckpointEngine(
            new List<IWeightedBonusPoolCheckpointStep>
            {
                new TieredWeightedBonusPoolCheckpointAdjuster(
                    new List<double> { 2227.0, 6853.0, 8398.0 },
                    new List<double> { 1.03, 1.11, 1.02 }),
                new MinimumWeightedBonusPoolCheckpointAdjuster(28.4),
                new FlaggedWeightedBonusPoolCheckpointAdjuster(flags, "weighted-bonus-pool-checkpoint-weighted", 0.7),
                new WeightedBonusPoolCheckpointWindowDaysOffsetAdjuster(0.85),
            },
            new List<string>(),
            0.2,
            39635.9);
    }

    public double Price(WeightedBonusPoolCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBonusPoolCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
