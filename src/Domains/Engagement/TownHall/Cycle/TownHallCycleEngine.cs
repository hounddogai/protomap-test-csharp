namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall cycle calculation.</summary>
public interface ITownHallCycleStep
{
    string Describe();
    double Adjust(TownHallCycle item, double valueValue);
}

public sealed record MinimumTownHallCycleAdjuster(double Minimum) : ITownHallCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTownHallCycleAdjuster(List<double> Bounds, List<double> Rates) : ITownHallCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallCycle item, double valueValue)
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

public sealed record CapTownHallCycleAdjuster(double Cap) : ITownHallCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTownHallCycleAdjuster(double Percent) : ITownHallCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall cycle steps in order and keeps a trace of each.</summary>
public sealed class TownHallCycleEngine(List<ITownHallCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallCycleEngine DefaultTownHallCycleEngine()
    {
        return new TownHallCycleEngine(
            new List<ITownHallCycleStep>
            {
                new MinimumTownHallCycleAdjuster(16.1),
                new TieredTownHallCycleAdjuster(
                    new List<double> { 1824.0, 4816.0, 7163.0 },
                    new List<double> { 1.04, 1.13, 1.16 }),
                new CapTownHallCycleAdjuster(32495.0),
                new PercentTownHallCycleAdjuster(5.7),
            },
            new List<string>(),
            1.8,
            69146.5);
    }

    public double Compute(TownHallCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
