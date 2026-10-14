namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal headcount reservation calculation.</summary>
public interface ISeasonalHeadcountReservationFactor
{
    string Describe();
    double Adjust(SeasonalHeadcountReservation item, double valueValue);
}

public sealed record RoundingSeasonalHeadcountReservationAdjuster(double Step) : ISeasonalHeadcountReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalHeadcountReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SeasonalHeadcountReservationRetriesOffsetAdjuster(double Weight) : ISeasonalHeadcountReservationFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(SeasonalHeadcountReservation item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record DepartmentSeasonalHeadcountReservationAdjuster(Dictionary<string, double> Factors) : ISeasonalHeadcountReservationFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SeasonalHeadcountReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalHeadcountReservationAdjuster(double Cap) : ISeasonalHeadcountReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalHeadcountReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalHeadcountReservationAdjuster(double Minimum) : ISeasonalHeadcountReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalHeadcountReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal headcount reservation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalHeadcountReservationEngine(List<ISeasonalHeadcountReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalHeadcountReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalHeadcountReservationEngine DefaultSeasonalHeadcountReservationEngine()
    {
        return new SeasonalHeadcountReservationEngine(
            new List<ISeasonalHeadcountReservationFactor>
            {
                new RoundingSeasonalHeadcountReservationAdjuster(0.05),
                new SeasonalHeadcountReservationRetriesOffsetAdjuster(0.6),
                new DepartmentSeasonalHeadcountReservationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.94, ["west"] = 1.0 }),
                new CapSeasonalHeadcountReservationAdjuster(20534.0),
                new MinimumSeasonalHeadcountReservationAdjuster(44.2),
            },
            new List<string>(),
            2.9,
            30053.2);
    }

    public double Evaluate(SeasonalHeadcountReservation item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalHeadcountReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
