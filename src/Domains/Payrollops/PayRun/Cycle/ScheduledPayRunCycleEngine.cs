namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pay run cycle calculation.</summary>
public interface IScheduledPayRunCycleFactor
{
    string Describe();
    double Adjust(ScheduledPayRunCycle item, double valueValue);
}

public sealed record TeamScheduledPayRunCycleAdjuster(Dictionary<string, double> Factors) : IScheduledPayRunCycleFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ScheduledPayRunCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScheduledPayRunCycleAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPayRunCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPayRunCycle item, double valueValue)
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

public sealed record FlaggedScheduledPayRunCycleAdjuster(Registry Flags, string Flag, double Percent) : IScheduledPayRunCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledPayRunCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledPayRunCycleAdjuster(double Step) : IScheduledPayRunCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPayRunCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScheduledPayRunCycleAdjuster(double Percent) : IScheduledPayRunCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPayRunCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled pay run cycle steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayRunCycleEngine(List<IScheduledPayRunCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayRunCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayRunCycleEngine DefaultScheduledPayRunCycleEngine(Registry flags)
    {
        return new ScheduledPayRunCycleEngine(
            new List<IScheduledPayRunCycleFactor>
            {
                new TeamScheduledPayRunCycleAdjuster(new Dictionary<string, double> { ["south"] = 1.06 }),
                new TieredScheduledPayRunCycleAdjuster(
                    new List<double> { 2087.0, 3710.0, 7555.0 },
                    new List<double> { 1.14, 1.08, 0.81 }),
                new FlaggedScheduledPayRunCycleAdjuster(flags, "scheduled-pay-run-cycle-weighted", 5.5),
                new RoundingScheduledPayRunCycleAdjuster(0.05),
                new PercentScheduledPayRunCycleAdjuster(-13.9),
            },
            new List<string>(),
            0.8,
            49078.4);
    }

    public double Evaluate(ScheduledPayRunCycle item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayRunCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
