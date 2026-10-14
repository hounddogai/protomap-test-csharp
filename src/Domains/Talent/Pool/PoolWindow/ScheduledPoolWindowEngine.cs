namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pool window calculation.</summary>
public interface IScheduledPoolWindowAdjuster
{
    string Describe();
    double Adjust(ScheduledPoolWindow item, double valueValue);
}

public sealed record FlaggedScheduledPoolWindowAdjuster(Registry Flags, string Flag, double Percent) : IScheduledPoolWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledPoolWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledPoolWindowAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPoolWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPoolWindow item, double valueValue)
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

public sealed record CapScheduledPoolWindowAdjuster(double Cap) : IScheduledPoolWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPoolWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledPoolWindowAdjuster(double Minimum) : IScheduledPoolWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPoolWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled pool window steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPoolWindowEngine(List<IScheduledPoolWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPoolWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPoolWindowEngine DefaultScheduledPoolWindowEngine(Registry flags)
    {
        return new ScheduledPoolWindowEngine(
            new List<IScheduledPoolWindowAdjuster>
            {
                new FlaggedScheduledPoolWindowAdjuster(flags, "scheduled-pool-window-primary", 4.3),
                new TieredScheduledPoolWindowAdjuster(
                    new List<double> { 1838.0, 4847.0, 6839.0, 8923.0 },
                    new List<double> { 0.88, 1.16, 1.05, 1.01 }),
                new CapScheduledPoolWindowAdjuster(2066.0),
                new MinimumScheduledPoolWindowAdjuster(32.8),
            },
            new List<string>(),
            2.7,
            42999.3);
    }

    public double Compute(ScheduledPoolWindow item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPoolWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
