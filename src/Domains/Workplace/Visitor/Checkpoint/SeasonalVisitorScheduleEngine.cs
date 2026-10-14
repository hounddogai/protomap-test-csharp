namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal visitor schedule calculation.</summary>
public interface ISeasonalVisitorScheduleModifier
{
    string Describe();
    double Adjust(SeasonalVisitorSchedule item, double valueValue);
}

public sealed record SeasonalVisitorScheduleStartsAtOffsetAdjuster(double Weight) : ISeasonalVisitorScheduleModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(SeasonalVisitorSchedule item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record RoundingSeasonalVisitorScheduleAdjuster(double Step) : ISeasonalVisitorScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalVisitorSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSeasonalVisitorScheduleAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalVisitorScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalVisitorSchedule item, double valueValue)
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

public sealed record PercentSeasonalVisitorScheduleAdjuster(double Percent) : ISeasonalVisitorScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalVisitorSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal visitor schedule steps in order and keeps a trace of each.</summary>
public sealed class SeasonalVisitorScheduleEngine(List<ISeasonalVisitorScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalVisitorScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalVisitorScheduleEngine DefaultSeasonalVisitorScheduleEngine()
    {
        return new SeasonalVisitorScheduleEngine(
            new List<ISeasonalVisitorScheduleModifier>
            {
                new SeasonalVisitorScheduleStartsAtOffsetAdjuster(1.19),
                new RoundingSeasonalVisitorScheduleAdjuster(0.05),
                new TieredSeasonalVisitorScheduleAdjuster(
                    new List<double> { 4585.0, 5878.0, 6387.0 },
                    new List<double> { 0.93, 0.96, 1.12 }),
                new PercentSeasonalVisitorScheduleAdjuster(-13.8),
            },
            new List<string>(),
            0.2,
            92165.3);
    }

    public double Compute(SeasonalVisitorSchedule item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalVisitorSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
