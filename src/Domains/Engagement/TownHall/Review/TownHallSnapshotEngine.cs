namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the town hall snapshot calculation.</summary>
public interface ITownHallSnapshotAdjuster
{
    string Describe();
    double Adjust(TownHallSnapshot item, double valueValue);
}

public sealed record MinimumTownHallSnapshotAdjuster(double Minimum) : ITownHallSnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTownHallSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ITownHallSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelTownHallSnapshotAdjuster(Dictionary<string, double> Factors) : ITownHallSnapshotAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TownHallSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TownHallSnapshotStartsAtOffsetAdjuster(double Weight) : ITownHallSnapshotAdjuster
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(TownHallSnapshot item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CapTownHallSnapshotAdjuster(double Cap) : ITownHallSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the town hall snapshot steps in order and keeps a trace of each.</summary>
public sealed class TownHallSnapshotEngine(List<ITownHallSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallSnapshotEngine DefaultTownHallSnapshotEngine(Registry flags)
    {
        return new TownHallSnapshotEngine(
            new List<ITownHallSnapshotAdjuster>
            {
                new MinimumTownHallSnapshotAdjuster(32.5),
                new FlaggedTownHallSnapshotAdjuster(flags, "town-hall-snapshot-legacy", -2.0),
                new LabelTownHallSnapshotAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.95, ["retail"] = 1.3 }),
                new TownHallSnapshotStartsAtOffsetAdjuster(1.77),
                new CapTownHallSnapshotAdjuster(42092.0),
            },
            new List<string>(),
            3.7,
            57956.8);
    }

    public double Compute(TownHallSnapshot item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
