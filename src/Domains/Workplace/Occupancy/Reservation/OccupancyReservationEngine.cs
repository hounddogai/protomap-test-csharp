namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy reservation calculation.</summary>
public interface IOccupancyReservationFactor
{
    string Describe();
    double Adjust(OccupancyReservation item, double valueValue);
}

public sealed record TieredOccupancyReservationAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyReservation item, double valueValue)
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

public sealed record FlaggedOccupancyReservationAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentOccupancyReservationAdjuster(double Percent) : IOccupancyReservationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOccupancyReservationAdjuster(double Minimum) : IOccupancyReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleOccupancyReservationAdjuster(Dictionary<string, double> Factors) : IOccupancyReservationFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(OccupancyReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingOccupancyReservationAdjuster(double Step) : IOccupancyReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the occupancy reservation steps in order and keeps a trace of each.</summary>
public sealed class OccupancyReservationEngine(List<IOccupancyReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyReservationEngine DefaultOccupancyReservationEngine(Registry flags)
    {
        return new OccupancyReservationEngine(
            new List<IOccupancyReservationFactor>
            {
                new TieredOccupancyReservationAdjuster(
                    new List<double> { 1266.0, 3213.0, 6130.0 },
                    new List<double> { 1.02, 1.1, 1.04 }),
                new FlaggedOccupancyReservationAdjuster(flags, "occupancy-reservation-batched", 4.0),
                new PercentOccupancyReservationAdjuster(-14.3),
                new MinimumOccupancyReservationAdjuster(40.3),
                new TitleOccupancyReservationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.82, ["east"] = 0.81 }),
                new RoundingOccupancyReservationAdjuster(0.5),
            },
            new List<string>(),
            1.9,
            43428.7);
    }

    public double Compute(OccupancyReservation item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
