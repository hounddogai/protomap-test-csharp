namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pay band threshold calculation.</summary>
public interface IScheduledPayBandThresholdAdjuster
{
    string Describe();
    double Adjust(ScheduledPayBandThreshold item, double valueValue);
}

public sealed record TitleScheduledPayBandThresholdAdjuster(Dictionary<string, double> Factors) : IScheduledPayBandThresholdAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledPayBandThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledPayBandThresholdAdjuster(Registry Flags, string Flag, double Percent) : IScheduledPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledPayBandThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentScheduledPayBandThresholdAdjuster(double Percent) : IScheduledPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPayBandThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledPayBandThresholdAdjuster(double Step) : IScheduledPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPayBandThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledPayBandThresholdAdjuster(double Minimum) : IScheduledPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPayBandThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledPayBandThresholdAdjuster(double Cap) : IScheduledPayBandThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPayBandThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled pay band threshold steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayBandThresholdEngine(List<IScheduledPayBandThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayBandThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayBandThresholdEngine DefaultScheduledPayBandThresholdEngine(Registry flags)
    {
        return new ScheduledPayBandThresholdEngine(
            new List<IScheduledPayBandThresholdAdjuster>
            {
                new TitleScheduledPayBandThresholdAdjuster(new Dictionary<string, double> { ["north"] = 1.32 }),
                new FlaggedScheduledPayBandThresholdAdjuster(flags, "scheduled-pay-band-threshold-seasonal", 8.2),
                new PercentScheduledPayBandThresholdAdjuster(-4.0),
                new RoundingScheduledPayBandThresholdAdjuster(5.0),
                new MinimumScheduledPayBandThresholdAdjuster(45.4),
                new CapScheduledPayBandThresholdAdjuster(16319.0),
            },
            new List<string>(),
            0.8,
            51506.0);
    }

    public double Price(ScheduledPayBandThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayBandThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
