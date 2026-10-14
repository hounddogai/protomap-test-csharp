namespace Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>One step of the town hall plan calculation.</summary>
public interface ITownHallPlanFactor
{
    string Describe();
    double Adjust(TownHallPlan item, double valueValue);
}

public sealed record TieredTownHallPlanAdjuster(List<double> Bounds, List<double> Rates) : ITownHallPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallPlan item, double valueValue)
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

public sealed record CapTownHallPlanAdjuster(double Cap) : ITownHallPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTownHallPlanAdjuster(double Minimum) : ITownHallPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the town hall plan steps in order and keeps a trace of each.</summary>
public sealed class TownHallPlanEngine(List<ITownHallPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallPlanEngine DefaultTownHallPlanEngine()
    {
        return new TownHallPlanEngine(
            new List<ITownHallPlanFactor>
            {
                new TieredTownHallPlanAdjuster(
                    new List<double> { 401.0, 1175.0, 1277.0, 2443.0 },
                    new List<double> { 1.14, 1.16, 1.06, 1.2 }),
                new CapTownHallPlanAdjuster(17220.0),
                new MinimumTownHallPlanAdjuster(18.0),
            },
            new List<string>(),
            1.0,
            58564.2);
    }

    public double Evaluate(TownHallPlan item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
