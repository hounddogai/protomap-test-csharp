namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy merit cycle checkpoint calculation.</summary>
public interface ILegacyMeritCycleCheckpointStep
{
    string Describe();
    double Adjust(LegacyMeritCycleCheckpoint item, double valueValue);
}

public sealed record TieredLegacyMeritCycleCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ILegacyMeritCycleCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyMeritCycleCheckpoint item, double valueValue)
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

public sealed record LegacyMeritCycleCheckpointVersionOffsetAdjuster(double Weight) : ILegacyMeritCycleCheckpointStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(LegacyMeritCycleCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumLegacyMeritCycleCheckpointAdjuster(double Minimum) : ILegacyMeritCycleCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyMeritCycleCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyMeritCycleCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ILegacyMeritCycleCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyMeritCycleCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLegacyMeritCycleCheckpointAdjuster(double Percent) : ILegacyMeritCycleCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyMeritCycleCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyMeritCycleCheckpointAdjuster(double Cap) : ILegacyMeritCycleCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyMeritCycleCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy merit cycle checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LegacyMeritCycleCheckpointEngine(List<ILegacyMeritCycleCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyMeritCycleCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyMeritCycleCheckpointEngine DefaultLegacyMeritCycleCheckpointEngine(Registry flags)
    {
        return new LegacyMeritCycleCheckpointEngine(
            new List<ILegacyMeritCycleCheckpointStep>
            {
                new TieredLegacyMeritCycleCheckpointAdjuster(
                    new List<double> { 1513.0, 3644.0, 6513.0, 8348.0 },
                    new List<double> { 1.03, 0.88, 0.89, 0.84 }),
                new LegacyMeritCycleCheckpointVersionOffsetAdjuster(0.86),
                new MinimumLegacyMeritCycleCheckpointAdjuster(40.3),
                new FlaggedLegacyMeritCycleCheckpointAdjuster(flags, "legacy-merit-cycle-checkpoint-regional", -5.8),
                new PercentLegacyMeritCycleCheckpointAdjuster(-7.8),
                new CapLegacyMeritCycleCheckpointAdjuster(24755.0),
            },
            new List<string>(),
            5.0,
            86430.5);
    }

    public double Evaluate(LegacyMeritCycleCheckpoint item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyMeritCycleCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
