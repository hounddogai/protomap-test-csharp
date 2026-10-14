namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>One step of the seasonal merit cycle reservation calculation.</summary>
public interface ISeasonalMeritCycleReservationAdjuster
{
    string Describe();
    double Adjust(SeasonalMeritCycleReservation item, double valueValue);
}

public sealed record CapSeasonalMeritCycleReservationAdjuster(double Cap) : ISeasonalMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalMeritCycleReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleSeasonalMeritCycleReservationAdjuster(Dictionary<string, double> Factors) : ISeasonalMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalMeritCycleReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalMeritCycleReservationAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalMeritCycleReservation item, double valueValue)
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

public sealed record SeasonalMeritCycleReservationThresholdOffsetAdjuster(double Weight) : ISeasonalMeritCycleReservationAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(SeasonalMeritCycleReservation item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the seasonal merit cycle reservation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalMeritCycleReservationEngine(List<ISeasonalMeritCycleReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalMeritCycleReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalMeritCycleReservationEngine DefaultSeasonalMeritCycleReservationEngine()
    {
        return new SeasonalMeritCycleReservationEngine(
            new List<ISeasonalMeritCycleReservationAdjuster>
            {
                new CapSeasonalMeritCycleReservationAdjuster(23640.0),
                new TitleSeasonalMeritCycleReservationAdjuster(new Dictionary<string, double> { ["east"] = 1.05 }),
                new TieredSeasonalMeritCycleReservationAdjuster(
                    new List<double> { 4159.0, 5049.0, 5166.0, 8366.0 },
                    new List<double> { 0.8, 0.92, 1.05, 0.87 }),
                new SeasonalMeritCycleReservationThresholdOffsetAdjuster(0.61),
            },
            new List<string>(),
            0.8,
            1364.7);
    }

    public double Evaluate(SeasonalMeritCycleReservation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalMeritCycleReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
