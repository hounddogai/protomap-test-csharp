namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal booking checkpoint calculation.</summary>
public interface ISeasonalBookingCheckpointStep
{
    string Describe();
    double Adjust(SeasonalBookingCheckpoint item, double valueValue);
}

public sealed record CapSeasonalBookingCheckpointAdjuster(double Cap) : ISeasonalBookingCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalBookingCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalBookingCheckpointAdjuster(double Step) : ISeasonalBookingCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalBookingCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelSeasonalBookingCheckpointAdjuster(Dictionary<string, double> Factors) : ISeasonalBookingCheckpointStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalBookingCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalBookingCheckpointAdjuster(double Percent) : ISeasonalBookingCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalBookingCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalBookingCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalBookingCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalBookingCheckpoint item, double valueValue)
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

/// <summary>Runs the seasonal booking checkpoint steps in order and keeps a trace of each.</summary>
public sealed class SeasonalBookingCheckpointEngine(List<ISeasonalBookingCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalBookingCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalBookingCheckpointEngine DefaultSeasonalBookingCheckpointEngine()
    {
        return new SeasonalBookingCheckpointEngine(
            new List<ISeasonalBookingCheckpointStep>
            {
                new CapSeasonalBookingCheckpointAdjuster(12362.0),
                new RoundingSeasonalBookingCheckpointAdjuster(5.0),
                new LabelSeasonalBookingCheckpointAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.11, ["central"] = 0.72, ["retail"] = 1.22 }),
                new PercentSeasonalBookingCheckpointAdjuster(14.6),
                new TieredSeasonalBookingCheckpointAdjuster(
                    new List<double> { 5455.0, 9005.0 },
                    new List<double> { 0.81, 0.84 }),
            },
            new List<string>(),
            0.2,
            14473.7);
    }

    public double Price(SeasonalBookingCheckpoint item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalBookingCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
