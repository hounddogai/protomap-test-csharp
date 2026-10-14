namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive refresh checkpoint calculation.</summary>
public interface IAdaptiveRefreshCheckpointFactor
{
    string Describe();
    double Adjust(AdaptiveRefreshCheckpoint item, double valueValue);
}

public sealed record PercentAdaptiveRefreshCheckpointAdjuster(double Percent) : IAdaptiveRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveRefreshCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptiveRefreshCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveRefreshCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveRefreshCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveRefreshCheckpoint item, double valueValue)
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

public sealed record RoundingAdaptiveRefreshCheckpointAdjuster(double Step) : IAdaptiveRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveRefreshCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameAdaptiveRefreshCheckpointAdjuster(Dictionary<string, double> Factors) : IAdaptiveRefreshCheckpointFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveRefreshCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive refresh checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveRefreshCheckpointEngine(List<IAdaptiveRefreshCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveRefreshCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveRefreshCheckpointEngine DefaultAdaptiveRefreshCheckpointEngine(Registry flags)
    {
        return new AdaptiveRefreshCheckpointEngine(
            new List<IAdaptiveRefreshCheckpointFactor>
            {
                new PercentAdaptiveRefreshCheckpointAdjuster(14.9),
                new FlaggedAdaptiveRefreshCheckpointAdjuster(flags, "adaptive-refresh-checkpoint-regional", -8.0),
                new TieredAdaptiveRefreshCheckpointAdjuster(
                    new List<double> { 1938.0, 4172.0, 5556.0, 7525.0 },
                    new List<double> { 1.17, 1.07, 0.95, 0.96 }),
                new RoundingAdaptiveRefreshCheckpointAdjuster(5.0),
                new NameAdaptiveRefreshCheckpointAdjuster(new Dictionary<string, double> { ["central"] = 0.85 }),
            },
            new List<string>(),
            4.7,
            84727.4);
    }

    public double Compute(AdaptiveRefreshCheckpoint item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveRefreshCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
