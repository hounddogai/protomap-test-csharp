namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pool threshold calculation.</summary>
public interface IScheduledPoolThresholdAdjuster
{
    string Describe();
    double Adjust(ScheduledPoolThreshold item, double valueValue);
}

public sealed record PercentScheduledPoolThresholdAdjuster(double Percent) : IScheduledPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPoolThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledPoolThresholdAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPoolThreshold item, double valueValue)
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

public sealed record MinimumScheduledPoolThresholdAdjuster(double Minimum) : IScheduledPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPoolThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScheduledPoolThresholdAdjuster(double Step) : IScheduledPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPoolThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled pool threshold steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPoolThresholdEngine(List<IScheduledPoolThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPoolThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPoolThresholdEngine DefaultScheduledPoolThresholdEngine()
    {
        return new ScheduledPoolThresholdEngine(
            new List<IScheduledPoolThresholdAdjuster>
            {
                new PercentScheduledPoolThresholdAdjuster(22.8),
                new TieredScheduledPoolThresholdAdjuster(
                    new List<double> { 275.0, 1124.0, 9588.0 },
                    new List<double> { 1.18, 1.15, 0.92 }),
                new MinimumScheduledPoolThresholdAdjuster(35.4),
                new RoundingScheduledPoolThresholdAdjuster(0.05),
            },
            new List<string>(),
            0.1,
            57449.3);
    }

    public double Price(ScheduledPoolThreshold item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPoolThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
