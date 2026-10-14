namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall estimate calculation.</summary>
public interface ITownHallEstimateStep
{
    string Describe();
    double Adjust(TownHallEstimate item, double valueValue);
}

public sealed record TownHallEstimateLeaveDaysOffsetAdjuster(double Weight) : ITownHallEstimateStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(TownHallEstimate item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record RoundingTownHallEstimateAdjuster(double Step) : ITownHallEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTownHallEstimateAdjuster(double Cap) : ITownHallEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the town hall estimate steps in order and keeps a trace of each.</summary>
public sealed class TownHallEstimateEngine(List<ITownHallEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallEstimateEngine DefaultTownHallEstimateEngine()
    {
        return new TownHallEstimateEngine(
            new List<ITownHallEstimateStep>
            {
                new TownHallEstimateLeaveDaysOffsetAdjuster(0.82),
                new RoundingTownHallEstimateAdjuster(0.1),
                new CapTownHallEstimateAdjuster(19599.0),
            },
            new List<string>(),
            0.6,
            20776.0);
    }

    public double Price(TownHallEstimate item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
