namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy town hall checkpoint calculation.</summary>
public interface ILegacyTownHallCheckpointModifier
{
    string Describe();
    double Adjust(LegacyTownHallCheckpoint item, double valueValue);
}

public sealed record RoundingLegacyTownHallCheckpointAdjuster(double Step) : ILegacyTownHallCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTownHallCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLegacyTownHallCheckpointAdjuster(double Cap) : ILegacyTownHallCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTownHallCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyTownHallCheckpointAdjuster(double Minimum) : ILegacyTownHallCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTownHallCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyTownHallCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTownHallCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTownHallCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyTownHallCheckpointFteOffsetAdjuster(double Weight) : ILegacyTownHallCheckpointModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(LegacyTownHallCheckpoint item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the legacy town hall checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LegacyTownHallCheckpointEngine(List<ILegacyTownHallCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTownHallCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTownHallCheckpointEngine DefaultLegacyTownHallCheckpointEngine(Registry flags)
    {
        return new LegacyTownHallCheckpointEngine(
            new List<ILegacyTownHallCheckpointModifier>
            {
                new RoundingLegacyTownHallCheckpointAdjuster(0.05),
                new CapLegacyTownHallCheckpointAdjuster(22804.0),
                new MinimumLegacyTownHallCheckpointAdjuster(35.1),
                new FlaggedLegacyTownHallCheckpointAdjuster(flags, "legacy-town-hall-checkpoint-seasonal", -5.1),
                new LegacyTownHallCheckpointFteOffsetAdjuster(0.35),
            },
            new List<string>(),
            4.2,
            67164.0);
    }

    public double Compute(LegacyTownHallCheckpoint item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTownHallCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
