namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled refresh cycle calculation.</summary>
public interface IScheduledRefreshCycleAdjuster
{
    string Describe();
    double Adjust(ScheduledRefreshCycle item, double valueValue);
}

public sealed record CapScheduledRefreshCycleAdjuster(double Cap) : IScheduledRefreshCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledRefreshCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledRefreshCycleAdjuster(double Minimum) : IScheduledRefreshCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledRefreshCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledRefreshCycleAdjuster(List<double> Bounds, List<double> Rates) : IScheduledRefreshCycleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledRefreshCycle item, double valueValue)
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

/// <summary>Runs the scheduled refresh cycle steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRefreshCycleEngine(List<IScheduledRefreshCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRefreshCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRefreshCycleEngine DefaultScheduledRefreshCycleEngine()
    {
        return new ScheduledRefreshCycleEngine(
            new List<IScheduledRefreshCycleAdjuster>
            {
                new CapScheduledRefreshCycleAdjuster(7374.0),
                new MinimumScheduledRefreshCycleAdjuster(3.0),
                new TieredScheduledRefreshCycleAdjuster(
                    new List<double> { 1039.0, 2647.0, 8598.0 },
                    new List<double> { 0.9, 1.19, 0.95 }),
            },
            new List<string>(),
            0.6,
            96022.9);
    }

    public double Run(ScheduledRefreshCycle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRefreshCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
