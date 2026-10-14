namespace Acme.Workforce.Workplace.Booking.Review;

/// <summary>One step of the weighted booking entry calculation.</summary>
public interface IWeightedBookingEntryModifier
{
    string Describe();
    double Adjust(WeightedBookingEntry item, double valueValue);
}

public sealed record TieredWeightedBookingEntryAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBookingEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBookingEntry item, double valueValue)
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

public sealed record PercentWeightedBookingEntryAdjuster(double Percent) : IWeightedBookingEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedBookingEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedBookingEntryVersionOffsetAdjuster(double Weight) : IWeightedBookingEntryModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(WeightedBookingEntry item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumWeightedBookingEntryAdjuster(double Minimum) : IWeightedBookingEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedBookingEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelWeightedBookingEntryAdjuster(Dictionary<string, double> Factors) : IWeightedBookingEntryModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WeightedBookingEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedBookingEntryAdjuster(double Cap) : IWeightedBookingEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedBookingEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted booking entry steps in order and keeps a trace of each.</summary>
public sealed class WeightedBookingEntryEngine(List<IWeightedBookingEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBookingEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBookingEntryEngine DefaultWeightedBookingEntryEngine()
    {
        return new WeightedBookingEntryEngine(
            new List<IWeightedBookingEntryModifier>
            {
                new TieredWeightedBookingEntryAdjuster(
                    new List<double> { 2493.0, 6530.0, 6770.0, 7831.0 },
                    new List<double> { 1.16, 0.92, 1.09, 0.86 }),
                new PercentWeightedBookingEntryAdjuster(-11.2),
                new WeightedBookingEntryVersionOffsetAdjuster(0.49),
                new MinimumWeightedBookingEntryAdjuster(16.1),
                new ChannelWeightedBookingEntryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.04, ["retail"] = 0.81, ["central"] = 1.38 }),
                new CapWeightedBookingEntryAdjuster(9715.0),
            },
            new List<string>(),
            3.0,
            57952.4);
    }

    public double Evaluate(WeightedBookingEntry item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBookingEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
