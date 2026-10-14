namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled adjustment allocation calculation.</summary>
public interface IScheduledAdjustmentAllocationStep
{
    string Describe();
    double Adjust(ScheduledAdjustmentAllocation item, double valueValue);
}

public sealed record TieredScheduledAdjustmentAllocationAdjuster(List<double> Bounds, List<double> Rates) : IScheduledAdjustmentAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledAdjustmentAllocation item, double valueValue)
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

public sealed record MinimumScheduledAdjustmentAllocationAdjuster(double Minimum) : IScheduledAdjustmentAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAdjustmentAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScheduledAdjustmentAllocationAdjuster(double Step) : IScheduledAdjustmentAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAdjustmentAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionScheduledAdjustmentAllocationAdjuster(Dictionary<string, double> Factors) : IScheduledAdjustmentAllocationStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ScheduledAdjustmentAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled adjustment allocation steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAdjustmentAllocationEngine(List<IScheduledAdjustmentAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAdjustmentAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAdjustmentAllocationEngine DefaultScheduledAdjustmentAllocationEngine()
    {
        return new ScheduledAdjustmentAllocationEngine(
            new List<IScheduledAdjustmentAllocationStep>
            {
                new TieredScheduledAdjustmentAllocationAdjuster(
                    new List<double> { 1065.0, 4143.0, 6714.0, 8500.0 },
                    new List<double> { 1.19, 0.95, 0.94, 0.92 }),
                new MinimumScheduledAdjustmentAllocationAdjuster(49.5),
                new RoundingScheduledAdjustmentAllocationAdjuster(0.25),
                new RegionScheduledAdjustmentAllocationAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.83, ["central"] = 0.94, ["east"] = 1.31 }),
            },
            new List<string>(),
            1.1,
            80341.4);
    }

    public double Price(ScheduledAdjustmentAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAdjustmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
