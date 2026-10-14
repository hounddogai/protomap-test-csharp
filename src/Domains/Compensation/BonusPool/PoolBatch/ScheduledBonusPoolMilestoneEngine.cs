namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled bonus pool milestone calculation.</summary>
public interface IScheduledBonusPoolMilestoneStep
{
    string Describe();
    double Adjust(ScheduledBonusPoolMilestone item, double valueValue);
}

public sealed record RoundingScheduledBonusPoolMilestoneAdjuster(double Step) : IScheduledBonusPoolMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledBonusPoolMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledBonusPoolMilestoneAdjuster(double Minimum) : IScheduledBonusPoolMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBonusPoolMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledBonusPoolMilestoneAdjuster(double Cap) : IScheduledBonusPoolMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledBonusPoolMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledBonusPoolMilestoneAdjuster(double Percent) : IScheduledBonusPoolMilestoneStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledBonusPoolMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled bonus pool milestone steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBonusPoolMilestoneEngine(List<IScheduledBonusPoolMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBonusPoolMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBonusPoolMilestoneEngine DefaultScheduledBonusPoolMilestoneEngine()
    {
        return new ScheduledBonusPoolMilestoneEngine(
            new List<IScheduledBonusPoolMilestoneStep>
            {
                new RoundingScheduledBonusPoolMilestoneAdjuster(1.0),
                new MinimumScheduledBonusPoolMilestoneAdjuster(44.0),
                new CapScheduledBonusPoolMilestoneAdjuster(20912.0),
                new PercentScheduledBonusPoolMilestoneAdjuster(-3.5),
            },
            new List<string>(),
            2.6,
            44505.0);
    }

    public double Compute(ScheduledBonusPoolMilestone item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBonusPoolMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
