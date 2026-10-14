namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback merit cycle checkpoint calculation.</summary>
public interface IFallbackMeritCycleCheckpointModifier
{
    string Describe();
    double Adjust(FallbackMeritCycleCheckpoint item, double valueValue);
}

public sealed record MinimumFallbackMeritCycleCheckpointAdjuster(double Minimum) : IFallbackMeritCycleCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackMeritCycleCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredFallbackMeritCycleCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IFallbackMeritCycleCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackMeritCycleCheckpoint item, double valueValue)
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

public sealed record FlaggedFallbackMeritCycleCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IFallbackMeritCycleCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackMeritCycleCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback merit cycle checkpoint steps in order and keeps a trace of each.</summary>
public sealed class FallbackMeritCycleCheckpointEngine(List<IFallbackMeritCycleCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackMeritCycleCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackMeritCycleCheckpointEngine DefaultFallbackMeritCycleCheckpointEngine(Registry flags)
    {
        return new FallbackMeritCycleCheckpointEngine(
            new List<IFallbackMeritCycleCheckpointModifier>
            {
                new MinimumFallbackMeritCycleCheckpointAdjuster(38.3),
                new TieredFallbackMeritCycleCheckpointAdjuster(
                    new List<double> { 5729.0, 9320.0 },
                    new List<double> { 0.84, 0.85 }),
                new FlaggedFallbackMeritCycleCheckpointAdjuster(
                    flags,
                    "fallback-merit-cycle-checkpoint-rolling",
                    8.7),
            },
            new List<string>(),
            2.1,
            69653.7);
    }

    public double Compute(FallbackMeritCycleCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackMeritCycleCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
