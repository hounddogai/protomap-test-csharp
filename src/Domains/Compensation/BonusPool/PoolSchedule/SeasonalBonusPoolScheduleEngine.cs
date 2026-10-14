namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal bonus pool schedule calculation.</summary>
public interface ISeasonalBonusPoolScheduleModifier
{
    string Describe();
    double Adjust(SeasonalBonusPoolSchedule item, double valueValue);
}

public sealed record RoundingSeasonalBonusPoolScheduleAdjuster(double Step) : ISeasonalBonusPoolScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalBonusPoolSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitSeasonalBonusPoolScheduleAdjuster(Dictionary<string, double> Factors) : ISeasonalBonusPoolScheduleModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(SeasonalBonusPoolSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SeasonalBonusPoolScheduleCostOffsetAdjuster(double Weight) : ISeasonalBonusPoolScheduleModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SeasonalBonusPoolSchedule item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the seasonal bonus pool schedule steps in order and keeps a trace of each.</summary>
public sealed class SeasonalBonusPoolScheduleEngine(List<ISeasonalBonusPoolScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalBonusPoolScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalBonusPoolScheduleEngine DefaultSeasonalBonusPoolScheduleEngine()
    {
        return new SeasonalBonusPoolScheduleEngine(
            new List<ISeasonalBonusPoolScheduleModifier>
            {
                new RoundingSeasonalBonusPoolScheduleAdjuster(5.0),
                new UnitSeasonalBonusPoolScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.11, ["east"] = 0.82 }),
                new SeasonalBonusPoolScheduleCostOffsetAdjuster(1.65),
            },
            new List<string>(),
            1.4,
            75537.6);
    }

    public double Run(SeasonalBonusPoolSchedule item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalBonusPoolSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
