namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary booking metric calculation.</summary>
public interface IPrimaryBookingMetricStep
{
    string Describe();
    double Adjust(PrimaryBookingMetric item, double valueValue);
}

public sealed record PercentPrimaryBookingMetricAdjuster(double Percent) : IPrimaryBookingMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryBookingMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryBookingMetricAdjuster(double Minimum) : IPrimaryBookingMetricStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryBookingMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPrimaryBookingMetricAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryBookingMetricStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryBookingMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryBookingMetricRatioOffsetAdjuster(double Weight) : IPrimaryBookingMetricStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(PrimaryBookingMetric item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the primary booking metric steps in order and keeps a trace of each.</summary>
public sealed class PrimaryBookingMetricEngine(List<IPrimaryBookingMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryBookingMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryBookingMetricEngine DefaultPrimaryBookingMetricEngine(Registry flags)
    {
        return new PrimaryBookingMetricEngine(
            new List<IPrimaryBookingMetricStep>
            {
                new PercentPrimaryBookingMetricAdjuster(8.2),
                new MinimumPrimaryBookingMetricAdjuster(24.3),
                new FlaggedPrimaryBookingMetricAdjuster(flags, "primary-booking-metric-batched", 0.2),
                new PrimaryBookingMetricRatioOffsetAdjuster(0.48),
            },
            new List<string>(),
            3.4,
            15606.8);
    }

    public double Run(PrimaryBookingMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryBookingMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
