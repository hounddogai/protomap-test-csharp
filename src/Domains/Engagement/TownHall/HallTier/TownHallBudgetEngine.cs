namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall budget calculation.</summary>
public interface ITownHallBudgetModifier
{
    string Describe();
    double Adjust(TownHallBudget item, double valueValue);
}

public sealed record TownHallBudgetWeightOffsetAdjuster(double Weight) : ITownHallBudgetModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(TownHallBudget item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record NameTownHallBudgetAdjuster(Dictionary<string, double> Factors) : ITownHallBudgetModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TownHallBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTownHallBudgetAdjuster(double Cap) : ITownHallBudgetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTownHallBudgetAdjuster(Registry Flags, string Flag, double Percent) : ITownHallBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall budget steps in order and keeps a trace of each.</summary>
public sealed class TownHallBudgetEngine(List<ITownHallBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallBudgetEngine DefaultTownHallBudgetEngine(Registry flags)
    {
        return new TownHallBudgetEngine(
            new List<ITownHallBudgetModifier>
            {
                new TownHallBudgetWeightOffsetAdjuster(0.18),
                new NameTownHallBudgetAdjuster(new Dictionary<string, double> { ["north"] = 0.8, ["south"] = 1.39 }),
                new CapTownHallBudgetAdjuster(43298.0),
                new FlaggedTownHallBudgetAdjuster(flags, "town-hall-budget-legacy", 4.9),
            },
            new List<string>(),
            0.9,
            45727.1);
    }

    public double Run(TownHallBudget item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
