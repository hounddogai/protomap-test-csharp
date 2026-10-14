namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal holiday claim calculation.</summary>
public interface ISeasonalHolidayClaimFactor
{
    string Describe();
    double Adjust(SeasonalHolidayClaim item, double valueValue);
}

public sealed record SegmentSeasonalHolidayClaimAdjuster(Dictionary<string, double> Factors) : ISeasonalHolidayClaimFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(SeasonalHolidayClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalHolidayClaimAdjuster(double Step) : ISeasonalHolidayClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalHolidayClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSeasonalHolidayClaimAdjuster(double Percent) : ISeasonalHolidayClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalHolidayClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalHolidayClaimRatingOffsetAdjuster(double Weight) : ISeasonalHolidayClaimFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(SeasonalHolidayClaim item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the seasonal holiday claim steps in order and keeps a trace of each.</summary>
public sealed class SeasonalHolidayClaimEngine(List<ISeasonalHolidayClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalHolidayClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalHolidayClaimEngine DefaultSeasonalHolidayClaimEngine()
    {
        return new SeasonalHolidayClaimEngine(
            new List<ISeasonalHolidayClaimFactor>
            {
                new SegmentSeasonalHolidayClaimAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.75, ["east"] = 1.39, ["online"] = 1.06 }),
                new RoundingSeasonalHolidayClaimAdjuster(5.0),
                new PercentSeasonalHolidayClaimAdjuster(21.9),
                new SeasonalHolidayClaimRatingOffsetAdjuster(0.89),
            },
            new List<string>(),
            4.1,
            62461.7);
    }

    public double Evaluate(SeasonalHolidayClaim item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalHolidayClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
