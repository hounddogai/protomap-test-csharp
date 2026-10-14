namespace Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>One step of the holiday reservation calculation.</summary>
public interface IHolidayReservationModifier
{
    string Describe();
    double Adjust(HolidayReservation item, double valueValue);
}

public sealed record CapHolidayReservationAdjuster(double Cap) : IHolidayReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentHolidayReservationAdjuster(double Percent) : IHolidayReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record HolidayReservationRateOffsetAdjuster(double Weight) : IHolidayReservationModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(HolidayReservation item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record TieredHolidayReservationAdjuster(List<double> Bounds, List<double> Rates) : IHolidayReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayReservation item, double valueValue)
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

/// <summary>Runs the holiday reservation steps in order and keeps a trace of each.</summary>
public sealed class HolidayReservationEngine(List<IHolidayReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayReservationEngine DefaultHolidayReservationEngine()
    {
        return new HolidayReservationEngine(
            new List<IHolidayReservationModifier>
            {
                new CapHolidayReservationAdjuster(43330.0),
                new PercentHolidayReservationAdjuster(13.6),
                new HolidayReservationRateOffsetAdjuster(0.33),
                new TieredHolidayReservationAdjuster(
                    new List<double> { 776.0, 4160.0, 8142.0, 9064.0 },
                    new List<double> { 1.03, 1.11, 0.9, 1.13 }),
            },
            new List<string>(),
            3.6,
            44369.7);
    }

    public double Evaluate(HolidayReservation item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
