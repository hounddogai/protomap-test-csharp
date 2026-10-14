namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall milestone calculation.</summary>
public interface ITownHallMilestoneStep
{
    string Describe();
    double Adjust(TownHallMilestone item, double valueValue);
}

public sealed record MinimumTownHallMilestoneAdjuster(double Minimum) : ITownHallMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTownHallMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ITownHallMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTownHallMilestoneAdjuster(double Percent) : ITownHallMilestoneStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTownHallMilestoneAdjuster(double Cap) : ITownHallMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTownHallMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ITownHallMilestoneStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallMilestone item, double valueValue)
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

public sealed record RoundingTownHallMilestoneAdjuster(double Step) : ITownHallMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the town hall milestone steps in order and keeps a trace of each.</summary>
public sealed class TownHallMilestoneEngine(List<ITownHallMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallMilestoneEngine DefaultTownHallMilestoneEngine(Registry flags)
    {
        return new TownHallMilestoneEngine(
            new List<ITownHallMilestoneStep>
            {
                new MinimumTownHallMilestoneAdjuster(43.6),
                new FlaggedTownHallMilestoneAdjuster(flags, "town-hall-milestone-batched", 8.7),
                new PercentTownHallMilestoneAdjuster(12.3),
                new CapTownHallMilestoneAdjuster(49201.0),
                new TieredTownHallMilestoneAdjuster(
                    new List<double> { 6718.0, 7958.0 },
                    new List<double> { 0.88, 1.12 }),
                new RoundingTownHallMilestoneAdjuster(5.0),
            },
            new List<string>(),
            4.5,
            58789.2);
    }

    public double Price(TownHallMilestone item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
