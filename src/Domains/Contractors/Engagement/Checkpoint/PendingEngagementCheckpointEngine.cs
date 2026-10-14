namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending engagement checkpoint calculation.</summary>
public interface IPendingEngagementCheckpointModifier
{
    string Describe();
    double Adjust(PendingEngagementCheckpoint item, double valueValue);
}

public sealed record CapPendingEngagementCheckpointAdjuster(double Cap) : IPendingEngagementCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingEngagementCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingEngagementCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IPendingEngagementCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingEngagementCheckpoint item, double valueValue)
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

public sealed record PercentPendingEngagementCheckpointAdjuster(double Percent) : IPendingEngagementCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingEngagementCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingEngagementCheckpointAdjuster(double Minimum) : IPendingEngagementCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingEngagementCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPendingEngagementCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IPendingEngagementCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingEngagementCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending engagement checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PendingEngagementCheckpointEngine(List<IPendingEngagementCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingEngagementCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingEngagementCheckpointEngine DefaultPendingEngagementCheckpointEngine(Registry flags)
    {
        return new PendingEngagementCheckpointEngine(
            new List<IPendingEngagementCheckpointModifier>
            {
                new CapPendingEngagementCheckpointAdjuster(12284.0),
                new TieredPendingEngagementCheckpointAdjuster(
                    new List<double> { 8811.0, 8962.0, 9739.0 },
                    new List<double> { 0.88, 0.95, 1.13 }),
                new PercentPendingEngagementCheckpointAdjuster(-22.7),
                new MinimumPendingEngagementCheckpointAdjuster(29.8),
                new FlaggedPendingEngagementCheckpointAdjuster(flags, "pending-engagement-checkpoint-default", -4.9),
            },
            new List<string>(),
            2.9,
            99860.4);
    }

    public double Compute(PendingEngagementCheckpoint item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingEngagementCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
