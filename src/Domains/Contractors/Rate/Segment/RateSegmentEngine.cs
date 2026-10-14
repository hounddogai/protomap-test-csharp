namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate segment calculation.</summary>
public interface IRateSegmentStep
{
    string Describe();
    double Adjust(RateSegment item, double valueValue);
}

public sealed record RateSegmentTenureMonthsOffsetAdjuster(double Weight) : IRateSegmentStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(RateSegment item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record PercentRateSegmentAdjuster(double Percent) : IRateSegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRateSegmentAdjuster(double Minimum) : IRateSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRateSegmentAdjuster(List<double> Bounds, List<double> Rates) : IRateSegmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateSegment item, double valueValue)
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

public sealed record CapRateSegmentAdjuster(double Cap) : IRateSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRateSegmentAdjuster(Registry Flags, string Flag, double Percent) : IRateSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate segment steps in order and keeps a trace of each.</summary>
public sealed class RateSegmentEngine(List<IRateSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateSegmentEngine DefaultRateSegmentEngine(Registry flags)
    {
        return new RateSegmentEngine(
            new List<IRateSegmentStep>
            {
                new RateSegmentTenureMonthsOffsetAdjuster(0.79),
                new PercentRateSegmentAdjuster(-16.6),
                new MinimumRateSegmentAdjuster(17.2),
                new TieredRateSegmentAdjuster(new List<double> { 2766.0, 9242.0 }, new List<double> { 0.84, 0.88 }),
                new CapRateSegmentAdjuster(39893.0),
                new FlaggedRateSegmentAdjuster(flags, "rate-segment-manual", 2.9),
            },
            new List<string>(),
            3.6,
            67738.9);
    }

    public double Price(RateSegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RateSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
