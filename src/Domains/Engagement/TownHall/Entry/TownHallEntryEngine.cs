namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall entry calculation.</summary>
public interface ITownHallEntryStep
{
    string Describe();
    double Adjust(TownHallEntry item, double valueValue);
}

public sealed record FlaggedTownHallEntryAdjuster(Registry Flags, string Flag, double Percent) : ITownHallEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTownHallEntryAdjuster(double Step) : ITownHallEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTownHallEntryAdjuster(double Cap) : ITownHallEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTownHallEntryAdjuster(double Minimum) : ITownHallEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the town hall entry steps in order and keeps a trace of each.</summary>
public sealed class TownHallEntryEngine(List<ITownHallEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallEntryEngine DefaultTownHallEntryEngine(Registry flags)
    {
        return new TownHallEntryEngine(
            new List<ITownHallEntryStep>
            {
                new FlaggedTownHallEntryAdjuster(flags, "town-hall-entry-scheduled", 4.3),
                new RoundingTownHallEntryAdjuster(0.25),
                new CapTownHallEntryAdjuster(33215.0),
                new MinimumTownHallEntryAdjuster(37.4),
            },
            new List<string>(),
            2.8,
            85350.8);
    }

    public double Evaluate(TownHallEntry item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
