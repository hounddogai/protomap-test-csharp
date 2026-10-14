namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional booking reservation calculation.</summary>
public interface IRegionalBookingReservationStep
{
    string Describe();
    double Adjust(RegionalBookingReservation item, double valueValue);
}

public sealed record MinimumRegionalBookingReservationAdjuster(double Minimum) : IRegionalBookingReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalBookingReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRegionalBookingReservationAdjuster(Dictionary<string, double> Factors) : IRegionalBookingReservationStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalBookingReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalBookingReservationAdjuster(double Percent) : IRegionalBookingReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalBookingReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalBookingReservationAdjuster(List<double> Bounds, List<double> Rates) : IRegionalBookingReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalBookingReservation item, double valueValue)
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

/// <summary>Runs the regional booking reservation steps in order and keeps a trace of each.</summary>
public sealed class RegionalBookingReservationEngine(List<IRegionalBookingReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalBookingReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalBookingReservationEngine DefaultRegionalBookingReservationEngine()
    {
        return new RegionalBookingReservationEngine(
            new List<IRegionalBookingReservationStep>
            {
                new MinimumRegionalBookingReservationAdjuster(33.7),
                new NameRegionalBookingReservationAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.0, ["online"] = 0.95 }),
                new PercentRegionalBookingReservationAdjuster(-16.5),
                new TieredRegionalBookingReservationAdjuster(
                    new List<double> { 6446.0, 6985.0, 9563.0 },
                    new List<double> { 0.94, 1.2, 1.02 }),
            },
            new List<string>(),
            4.5,
            20016.5);
    }

    public double Price(RegionalBookingReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalBookingReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
