namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global town hall checkpoint calculation.</summary>
public interface IGlobalTownHallCheckpointFactor
{
    string Describe();
    double Adjust(GlobalTownHallCheckpoint item, double valueValue);
}

public sealed record TeamGlobalTownHallCheckpointAdjuster(Dictionary<string, double> Factors) : IGlobalTownHallCheckpointFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(GlobalTownHallCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalTownHallCheckpointAdjuster(double Minimum) : IGlobalTownHallCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalTownHallCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentGlobalTownHallCheckpointAdjuster(double Percent) : IGlobalTownHallCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalTownHallCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedGlobalTownHallCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IGlobalTownHallCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalTownHallCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global town hall checkpoint steps in order and keeps a trace of each.</summary>
public sealed class GlobalTownHallCheckpointEngine(List<IGlobalTownHallCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalTownHallCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalTownHallCheckpointEngine DefaultGlobalTownHallCheckpointEngine(Registry flags)
    {
        return new GlobalTownHallCheckpointEngine(
            new List<IGlobalTownHallCheckpointFactor>
            {
                new TeamGlobalTownHallCheckpointAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.23, ["east"] = 1.01, ["south"] = 1.15 }),
                new MinimumGlobalTownHallCheckpointAdjuster(34.2),
                new PercentGlobalTownHallCheckpointAdjuster(-22.8),
                new FlaggedGlobalTownHallCheckpointAdjuster(flags, "global-town-hall-checkpoint-adaptive", -7.6),
            },
            new List<string>(),
            4.8,
            13641.4);
    }

    public double Run(GlobalTownHallCheckpoint item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalTownHallCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
