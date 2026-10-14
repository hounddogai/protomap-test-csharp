namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched adjustment schedule calculation.</summary>
public interface IBatchedAdjustmentScheduleModifier
{
    string Describe();
    double Adjust(BatchedAdjustmentSchedule item, double valueValue);
}

public sealed record BatchedAdjustmentScheduleExpiresAtOffsetAdjuster(double Weight) : IBatchedAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(BatchedAdjustmentSchedule item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingBatchedAdjustmentScheduleAdjuster(double Step) : IBatchedAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedAdjustmentSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedAdjustmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAdjustmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelBatchedAdjustmentScheduleAdjuster(Dictionary<string, double> Factors) : IBatchedAdjustmentScheduleModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BatchedAdjustmentSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedAdjustmentScheduleAdjuster(List<double> Bounds, List<double> Rates) : IBatchedAdjustmentScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedAdjustmentSchedule item, double valueValue)
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

/// <summary>Runs the batched adjustment schedule steps in order and keeps a trace of each.</summary>
public sealed class BatchedAdjustmentScheduleEngine(List<IBatchedAdjustmentScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAdjustmentScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAdjustmentScheduleEngine DefaultBatchedAdjustmentScheduleEngine(Registry flags)
    {
        return new BatchedAdjustmentScheduleEngine(
            new List<IBatchedAdjustmentScheduleModifier>
            {
                new BatchedAdjustmentScheduleExpiresAtOffsetAdjuster(0.93),
                new RoundingBatchedAdjustmentScheduleAdjuster(0.25),
                new FlaggedBatchedAdjustmentScheduleAdjuster(flags, "batched-adjustment-schedule-legacy", -8.8),
                new LabelBatchedAdjustmentScheduleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.78, ["east"] = 1.21, ["central"] = 0.78 }),
                new TieredBatchedAdjustmentScheduleAdjuster(
                    new List<double> { 1777.0, 4703.0 },
                    new List<double> { 1.03, 0.85 }),
            },
            new List<string>(),
            3.1,
            93973.7);
    }

    public double Compute(BatchedAdjustmentSchedule item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAdjustmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
