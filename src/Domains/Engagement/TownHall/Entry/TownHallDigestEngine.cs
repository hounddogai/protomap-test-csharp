namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the town hall digest calculation.</summary>
public interface ITownHallDigestFactor
{
    string Describe();
    double Adjust(TownHallDigest item, double valueValue);
}

public sealed record FlaggedTownHallDigestAdjuster(Registry Flags, string Flag, double Percent) : ITownHallDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelTownHallDigestAdjuster(Dictionary<string, double> Factors) : ITownHallDigestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TownHallDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTownHallDigestAdjuster(double Cap) : ITownHallDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTownHallDigestAdjuster(double Minimum) : ITownHallDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the town hall digest steps in order and keeps a trace of each.</summary>
public sealed class TownHallDigestEngine(List<ITownHallDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallDigestEngine DefaultTownHallDigestEngine(Registry flags)
    {
        return new TownHallDigestEngine(
            new List<ITownHallDigestFactor>
            {
                new FlaggedTownHallDigestAdjuster(flags, "town-hall-digest-regional", 2.2),
                new LabelTownHallDigestAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["south"] = 1.05, ["west"] = 1.25 }),
                new CapTownHallDigestAdjuster(40593.0),
                new MinimumTownHallDigestAdjuster(27.2),
            },
            new List<string>(),
            1.0,
            28563.4);
    }

    public double Compute(TownHallDigest item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
