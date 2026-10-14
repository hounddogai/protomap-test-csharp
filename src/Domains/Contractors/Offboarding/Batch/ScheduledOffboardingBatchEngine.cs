namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled offboarding batch calculation.</summary>
public interface IScheduledOffboardingBatchStep
{
    string Describe();
    double Adjust(ScheduledOffboardingBatch item, double valueValue);
}

public sealed record FlaggedScheduledOffboardingBatchAdjuster(Registry Flags, string Flag, double Percent) : IScheduledOffboardingBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledOffboardingBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledOffboardingBatchAdjuster(double Step) : IScheduledOffboardingBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledOffboardingBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ScheduledOffboardingBatchBatchSizeOffsetAdjuster(double Weight) : IScheduledOffboardingBatchStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ScheduledOffboardingBatch item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record NameScheduledOffboardingBatchAdjuster(Dictionary<string, double> Factors) : IScheduledOffboardingBatchStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledOffboardingBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledOffboardingBatchAdjuster(double Cap) : IScheduledOffboardingBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledOffboardingBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledOffboardingBatchAdjuster(double Minimum) : IScheduledOffboardingBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledOffboardingBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled offboarding batch steps in order and keeps a trace of each.</summary>
public sealed class ScheduledOffboardingBatchEngine(List<IScheduledOffboardingBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledOffboardingBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledOffboardingBatchEngine DefaultScheduledOffboardingBatchEngine(Registry flags)
    {
        return new ScheduledOffboardingBatchEngine(
            new List<IScheduledOffboardingBatchStep>
            {
                new FlaggedScheduledOffboardingBatchAdjuster(flags, "scheduled-offboarding-batch-pending", -2.8),
                new RoundingScheduledOffboardingBatchAdjuster(0.5),
                new ScheduledOffboardingBatchBatchSizeOffsetAdjuster(0.29),
                new NameScheduledOffboardingBatchAdjuster(new Dictionary<string, double> { ["east"] = 0.78 }),
                new CapScheduledOffboardingBatchAdjuster(13829.0),
                new MinimumScheduledOffboardingBatchAdjuster(4.8),
            },
            new List<string>(),
            2.8,
            16879.9);
    }

    public double Run(ScheduledOffboardingBatch item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledOffboardingBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
