namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted booking variance calculation.</summary>
public interface IWeightedBookingVarianceFactor
{
    string Describe();
    double Adjust(WeightedBookingVariance item, double valueValue);
}

public sealed record ChannelWeightedBookingVarianceAdjuster(Dictionary<string, double> Factors) : IWeightedBookingVarianceFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WeightedBookingVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedBookingVarianceAdjuster(double Minimum) : IWeightedBookingVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedBookingVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedBookingVarianceAdjuster(double Step) : IWeightedBookingVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedBookingVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWeightedBookingVarianceAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBookingVarianceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBookingVariance item, double valueValue)
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

/// <summary>Runs the weighted booking variance steps in order and keeps a trace of each.</summary>
public sealed class WeightedBookingVarianceEngine(List<IWeightedBookingVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBookingVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBookingVarianceEngine DefaultWeightedBookingVarianceEngine()
    {
        return new WeightedBookingVarianceEngine(
            new List<IWeightedBookingVarianceFactor>
            {
                new ChannelWeightedBookingVarianceAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.81, ["retail"] = 1.17, ["central"] = 1.25 }),
                new MinimumWeightedBookingVarianceAdjuster(36.7),
                new RoundingWeightedBookingVarianceAdjuster(1.0),
                new TieredWeightedBookingVarianceAdjuster(
                    new List<double> { 2913.0, 5646.0, 5834.0, 8756.0 },
                    new List<double> { 0.85, 1.12, 1.06, 1.14 }),
            },
            new List<string>(),
            0.3,
            89778.2);
    }

    public double Run(WeightedBookingVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBookingVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
