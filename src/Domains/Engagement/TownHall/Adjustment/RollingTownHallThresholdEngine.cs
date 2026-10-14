namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling town hall threshold calculation.</summary>
public interface IRollingTownHallThresholdModifier
{
    string Describe();
    double Adjust(RollingTownHallThreshold item, double valueValue);
}

public sealed record CapRollingTownHallThresholdAdjuster(double Cap) : IRollingTownHallThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingTownHallThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingTownHallThresholdAdjuster(Registry Flags, string Flag, double Percent) : IRollingTownHallThresholdModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingTownHallThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingTownHallThresholdAdjuster(double Minimum) : IRollingTownHallThresholdModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingTownHallThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRollingTownHallThresholdAdjuster(List<double> Bounds, List<double> Rates) : IRollingTownHallThresholdModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingTownHallThreshold item, double valueValue)
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

public sealed record SourceRollingTownHallThresholdAdjuster(Dictionary<string, double> Factors) : IRollingTownHallThresholdModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RollingTownHallThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRollingTownHallThresholdAdjuster(double Step) : IRollingTownHallThresholdModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingTownHallThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling town hall threshold steps in order and keeps a trace of each.</summary>
public sealed class RollingTownHallThresholdEngine(List<IRollingTownHallThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingTownHallThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingTownHallThresholdEngine DefaultRollingTownHallThresholdEngine(Registry flags)
    {
        return new RollingTownHallThresholdEngine(
            new List<IRollingTownHallThresholdModifier>
            {
                new CapRollingTownHallThresholdAdjuster(20543.0),
                new FlaggedRollingTownHallThresholdAdjuster(flags, "rolling-town-hall-threshold-legacy", 1.9),
                new MinimumRollingTownHallThresholdAdjuster(30.1),
                new TieredRollingTownHallThresholdAdjuster(
                    new List<double> { 90.0, 5506.0, 5737.0, 9904.0 },
                    new List<double> { 0.8, 0.92, 0.96, 0.92 }),
                new SourceRollingTownHallThresholdAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["retail"] = 1.31, ["north"] = 0.97 }),
                new RoundingRollingTownHallThresholdAdjuster(0.1),
            },
            new List<string>(),
            4.1,
            63519.5);
    }

    public double Evaluate(RollingTownHallThreshold item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingTownHallThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
