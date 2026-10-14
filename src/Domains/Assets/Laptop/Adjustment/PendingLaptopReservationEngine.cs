namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending laptop reservation calculation.</summary>
public interface IPendingLaptopReservationModifier
{
    string Describe();
    double Adjust(PendingLaptopReservation item, double valueValue);
}

public sealed record FlaggedPendingLaptopReservationAdjuster(Registry Flags, string Flag, double Percent) : IPendingLaptopReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingLaptopReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPendingLaptopReservationAdjuster(double Percent) : IPendingLaptopReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingLaptopReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingLaptopReservationAdjuster(List<double> Bounds, List<double> Rates) : IPendingLaptopReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingLaptopReservation item, double valueValue)
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

public sealed record CapPendingLaptopReservationAdjuster(double Cap) : IPendingLaptopReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingLaptopReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPendingLaptopReservationAdjuster(double Step) : IPendingLaptopReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingLaptopReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryPendingLaptopReservationAdjuster(Dictionary<string, double> Factors) : IPendingLaptopReservationModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PendingLaptopReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending laptop reservation steps in order and keeps a trace of each.</summary>
public sealed class PendingLaptopReservationEngine(List<IPendingLaptopReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingLaptopReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingLaptopReservationEngine DefaultPendingLaptopReservationEngine(Registry flags)
    {
        return new PendingLaptopReservationEngine(
            new List<IPendingLaptopReservationModifier>
            {
                new FlaggedPendingLaptopReservationAdjuster(flags, "pending-laptop-reservation-weighted", 1.7),
                new PercentPendingLaptopReservationAdjuster(24.8),
                new TieredPendingLaptopReservationAdjuster(
                    new List<double> { 3384.0, 4322.0 },
                    new List<double> { 0.95, 1.19 }),
                new CapPendingLaptopReservationAdjuster(25492.0),
                new RoundingPendingLaptopReservationAdjuster(0.1),
                new CountryPendingLaptopReservationAdjuster(new Dictionary<string, double> { ["south"] = 1.27 }),
            },
            new List<string>(),
            0.8,
            97940.0);
    }

    public double Price(PendingLaptopReservation item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingLaptopReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
