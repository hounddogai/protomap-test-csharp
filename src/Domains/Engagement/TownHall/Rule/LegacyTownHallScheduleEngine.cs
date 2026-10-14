namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy town hall schedule calculation.</summary>
public interface ILegacyTownHallScheduleAdjuster
{
    string Describe();
    double Adjust(LegacyTownHallSchedule item, double valueValue);
}

public sealed record FlaggedLegacyTownHallScheduleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTownHallScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTownHallSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceLegacyTownHallScheduleAdjuster(Dictionary<string, double> Factors) : ILegacyTownHallScheduleAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LegacyTownHallSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacyTownHallScheduleAdjuster(double Step) : ILegacyTownHallScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTownHallSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy town hall schedule steps in order and keeps a trace of each.</summary>
public sealed class LegacyTownHallScheduleEngine(List<ILegacyTownHallScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTownHallScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTownHallScheduleEngine DefaultLegacyTownHallScheduleEngine(Registry flags)
    {
        return new LegacyTownHallScheduleEngine(
            new List<ILegacyTownHallScheduleAdjuster>
            {
                new FlaggedLegacyTownHallScheduleAdjuster(flags, "legacy-town-hall-schedule-rolling", 1.6),
                new SourceLegacyTownHallScheduleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.74, ["west"] = 0.85 }),
                new RoundingLegacyTownHallScheduleAdjuster(5.0),
            },
            new List<string>(),
            0.2,
            42813.1);
    }

    public double Evaluate(LegacyTownHallSchedule item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTownHallSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
