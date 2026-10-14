namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall checkpoint calculation.</summary>
public interface ITownHallCheckpointModifier
{
    string Describe();
    double Adjust(TownHallCheckpoint item, double valueValue);
}

public sealed record MinimumTownHallCheckpointAdjuster(double Minimum) : ITownHallCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTownHallCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ITownHallCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionTownHallCheckpointAdjuster(Dictionary<string, double> Factors) : ITownHallCheckpointModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TownHallCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTownHallCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ITownHallCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallCheckpoint item, double valueValue)
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

public sealed record PercentTownHallCheckpointAdjuster(double Percent) : ITownHallCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall checkpoint steps in order and keeps a trace of each.</summary>
public sealed class TownHallCheckpointEngine(List<ITownHallCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallCheckpointEngine DefaultTownHallCheckpointEngine(Registry flags)
    {
        return new TownHallCheckpointEngine(
            new List<ITownHallCheckpointModifier>
            {
                new MinimumTownHallCheckpointAdjuster(49.5),
                new FlaggedTownHallCheckpointAdjuster(flags, "town-hall-checkpoint-scheduled", 3.9),
                new RegionTownHallCheckpointAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.38, ["east"] = 0.74 }),
                new TieredTownHallCheckpointAdjuster(
                    new List<double> { 5190.0, 5190.0, 8013.0 },
                    new List<double> { 1.15, 0.9, 1.14 }),
                new PercentTownHallCheckpointAdjuster(24.1),
            },
            new List<string>(),
            4.6,
            69215.0);
    }

    public double Run(TownHallCheckpoint item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
