namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy town hall cycle calculation.</summary>
public interface ILegacyTownHallCycleModifier
{
    string Describe();
    double Adjust(LegacyTownHallCycle item, double valueValue);
}

public sealed record MinimumLegacyTownHallCycleAdjuster(double Minimum) : ILegacyTownHallCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTownHallCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyTownHallCycleAdjuster(double Cap) : ILegacyTownHallCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTownHallCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleLegacyTownHallCycleAdjuster(Dictionary<string, double> Factors) : ILegacyTownHallCycleModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyTownHallCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyTownHallCycleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTownHallCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTownHallCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy town hall cycle steps in order and keeps a trace of each.</summary>
public sealed class LegacyTownHallCycleEngine(List<ILegacyTownHallCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTownHallCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTownHallCycleEngine DefaultLegacyTownHallCycleEngine(Registry flags)
    {
        return new LegacyTownHallCycleEngine(
            new List<ILegacyTownHallCycleModifier>
            {
                new MinimumLegacyTownHallCycleAdjuster(23.7),
                new CapLegacyTownHallCycleAdjuster(49136.0),
                new TitleLegacyTownHallCycleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.15, ["retail"] = 0.74 }),
                new FlaggedLegacyTownHallCycleAdjuster(flags, "legacy-town-hall-cycle-primary", -1.5),
            },
            new List<string>(),
            0.5,
            17121.6);
    }

    public double Evaluate(LegacyTownHallCycle item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTownHallCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
