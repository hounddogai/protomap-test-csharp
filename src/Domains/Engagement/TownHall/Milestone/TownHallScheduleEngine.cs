namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>One step of the town hall schedule calculation.</summary>
public interface ITownHallScheduleAdjuster
{
    string Describe();
    double Adjust(TownHallSchedule item, double valueValue);
}

public sealed record TieredTownHallScheduleAdjuster(List<double> Bounds, List<double> Rates) : ITownHallScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallSchedule item, double valueValue)
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

public sealed record LabelTownHallScheduleAdjuster(Dictionary<string, double> Factors) : ITownHallScheduleAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TownHallSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTownHallScheduleAdjuster(double Cap) : ITownHallScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the town hall schedule steps in order and keeps a trace of each.</summary>
public sealed class TownHallScheduleEngine(List<ITownHallScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallScheduleEngine DefaultTownHallScheduleEngine()
    {
        return new TownHallScheduleEngine(
            new List<ITownHallScheduleAdjuster>
            {
                new TieredTownHallScheduleAdjuster(
                    new List<double> { 4433.0, 6011.0 },
                    new List<double> { 0.84, 0.98 }),
                new LabelTownHallScheduleAdjuster(new Dictionary<string, double> { ["online"] = 1.23 }),
                new CapTownHallScheduleAdjuster(20432.0),
            },
            new List<string>(),
            0.2,
            57597.8);
    }

    public double Price(TownHallSchedule item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
