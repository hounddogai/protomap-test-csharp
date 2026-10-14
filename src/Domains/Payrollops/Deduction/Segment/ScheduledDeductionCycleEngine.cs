namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled deduction cycle calculation.</summary>
public interface IScheduledDeductionCycleFactor
{
    string Describe();
    double Adjust(ScheduledDeductionCycle item, double valueValue);
}

public sealed record UnitScheduledDeductionCycleAdjuster(Dictionary<string, double> Factors) : IScheduledDeductionCycleFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ScheduledDeductionCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScheduledDeductionCycleThresholdOffsetAdjuster(double Weight) : IScheduledDeductionCycleFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ScheduledDeductionCycle item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedScheduledDeductionCycleAdjuster(Registry Flags, string Flag, double Percent) : IScheduledDeductionCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledDeductionCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled deduction cycle steps in order and keeps a trace of each.</summary>
public sealed class ScheduledDeductionCycleEngine(List<IScheduledDeductionCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledDeductionCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledDeductionCycleEngine DefaultScheduledDeductionCycleEngine(Registry flags)
    {
        return new ScheduledDeductionCycleEngine(
            new List<IScheduledDeductionCycleFactor>
            {
                new UnitScheduledDeductionCycleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.77, ["central"] = 0.75, ["south"] = 1.23 }),
                new ScheduledDeductionCycleThresholdOffsetAdjuster(1.21),
                new FlaggedScheduledDeductionCycleAdjuster(flags, "scheduled-deduction-cycle-manual", 6.1),
            },
            new List<string>(),
            3.5,
            55881.8);
    }

    public double Evaluate(ScheduledDeductionCycle item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledDeductionCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
