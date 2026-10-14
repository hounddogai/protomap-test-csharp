namespace Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>One step of the town hall request calculation.</summary>
public interface ITownHallRequestAdjuster
{
    string Describe();
    double Adjust(TownHallRequest item, double valueValue);
}

public sealed record CapTownHallRequestAdjuster(double Cap) : ITownHallRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTownHallRequestAdjuster(double Percent) : ITownHallRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TownHallRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTownHallRequestAdjuster(List<double> Bounds, List<double> Rates) : ITownHallRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallRequest item, double valueValue)
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

/// <summary>Runs the town hall request steps in order and keeps a trace of each.</summary>
public sealed class TownHallRequestEngine(List<ITownHallRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallRequestEngine DefaultTownHallRequestEngine()
    {
        return new TownHallRequestEngine(
            new List<ITownHallRequestAdjuster>
            {
                new CapTownHallRequestAdjuster(34893.0),
                new PercentTownHallRequestAdjuster(5.4),
                new TieredTownHallRequestAdjuster(
                    new List<double> { 5878.0, 6415.0, 7112.0 },
                    new List<double> { 1.07, 1.08, 0.98 }),
            },
            new List<string>(),
            3.2,
            25770.4);
    }

    public double Compute(TownHallRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
