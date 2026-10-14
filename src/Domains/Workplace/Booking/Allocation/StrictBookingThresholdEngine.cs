namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict booking threshold calculation.</summary>
public interface IStrictBookingThresholdAdjuster
{
    string Describe();
    double Adjust(StrictBookingThreshold item, double valueValue);
}

public sealed record RoundingStrictBookingThresholdAdjuster(double Step) : IStrictBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictBookingThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedStrictBookingThresholdAdjuster(Registry Flags, string Flag, double Percent) : IStrictBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictBookingThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictBookingThresholdAdjuster(double Minimum) : IStrictBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictBookingThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictBookingThresholdLimitOffsetAdjuster(double Weight) : IStrictBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(StrictBookingThreshold item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CapStrictBookingThresholdAdjuster(double Cap) : IStrictBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictBookingThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictBookingThresholdAdjuster(List<double> Bounds, List<double> Rates) : IStrictBookingThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictBookingThreshold item, double valueValue)
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

/// <summary>Runs the strict booking threshold steps in order and keeps a trace of each.</summary>
public sealed class StrictBookingThresholdEngine(List<IStrictBookingThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictBookingThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictBookingThresholdEngine DefaultStrictBookingThresholdEngine(Registry flags)
    {
        return new StrictBookingThresholdEngine(
            new List<IStrictBookingThresholdAdjuster>
            {
                new RoundingStrictBookingThresholdAdjuster(0.1),
                new FlaggedStrictBookingThresholdAdjuster(flags, "strict-booking-threshold-batched", 1.6),
                new MinimumStrictBookingThresholdAdjuster(17.3),
                new StrictBookingThresholdLimitOffsetAdjuster(1.73),
                new CapStrictBookingThresholdAdjuster(40078.0),
                new TieredStrictBookingThresholdAdjuster(
                    new List<double> { 470.0, 2489.0, 5008.0, 6628.0 },
                    new List<double> { 1.18, 0.96, 0.87, 0.81 }),
            },
            new List<string>(),
            0.9,
            97689.2);
    }

    public double Compute(StrictBookingThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictBookingThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
