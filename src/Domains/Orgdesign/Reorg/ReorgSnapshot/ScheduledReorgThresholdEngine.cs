namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled reorg threshold calculation.</summary>
public interface IScheduledReorgThresholdFactor
{
    string Describe();
    double Adjust(ScheduledReorgThreshold item, double valueValue);
}

public sealed record PercentScheduledReorgThresholdAdjuster(double Percent) : IScheduledReorgThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledReorgThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelScheduledReorgThresholdAdjuster(Dictionary<string, double> Factors) : IScheduledReorgThresholdFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledReorgThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScheduledReorgThresholdAdjuster(List<double> Bounds, List<double> Rates) : IScheduledReorgThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledReorgThreshold item, double valueValue)
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

/// <summary>Runs the scheduled reorg threshold steps in order and keeps a trace of each.</summary>
public sealed class ScheduledReorgThresholdEngine(List<IScheduledReorgThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledReorgThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledReorgThresholdEngine DefaultScheduledReorgThresholdEngine()
    {
        return new ScheduledReorgThresholdEngine(
            new List<IScheduledReorgThresholdFactor>
            {
                new PercentScheduledReorgThresholdAdjuster(-1.5),
                new LabelScheduledReorgThresholdAdjuster(new Dictionary<string, double> { ["south"] = 1.36 }),
                new TieredScheduledReorgThresholdAdjuster(
                    new List<double> { 2158.0, 3878.0 },
                    new List<double> { 0.84, 1.0 }),
            },
            new List<string>(),
            3.5,
            77486.7);
    }

    public double Compute(ScheduledReorgThreshold item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledReorgThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
