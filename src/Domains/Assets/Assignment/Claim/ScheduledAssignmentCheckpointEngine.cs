namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled assignment checkpoint calculation.</summary>
public interface IScheduledAssignmentCheckpointStep
{
    string Describe();
    double Adjust(ScheduledAssignmentCheckpoint item, double valueValue);
}

public sealed record SourceScheduledAssignmentCheckpointAdjuster(Dictionary<string, double> Factors) : IScheduledAssignmentCheckpointStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(ScheduledAssignmentCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumScheduledAssignmentCheckpointAdjuster(double Minimum) : IScheduledAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAssignmentCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledAssignmentCheckpointAdjuster(double Cap) : IScheduledAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAssignmentCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledAssignmentCheckpointAdjuster(double Step) : IScheduledAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAssignmentCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledAssignmentCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAssignmentCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAssignmentCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled assignment checkpoint steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAssignmentCheckpointEngine(List<IScheduledAssignmentCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAssignmentCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAssignmentCheckpointEngine DefaultScheduledAssignmentCheckpointEngine(Registry flags)
    {
        return new ScheduledAssignmentCheckpointEngine(
            new List<IScheduledAssignmentCheckpointStep>
            {
                new SourceScheduledAssignmentCheckpointAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.23, ["east"] = 1.37, ["south"] = 1.23 }),
                new MinimumScheduledAssignmentCheckpointAdjuster(41.6),
                new CapScheduledAssignmentCheckpointAdjuster(6842.0),
                new RoundingScheduledAssignmentCheckpointAdjuster(0.05),
                new FlaggedScheduledAssignmentCheckpointAdjuster(
                    flags,
                    "scheduled-assignment-checkpoint-seasonal",
                    0.3),
            },
            new List<string>(),
            1.7,
            31273.6);
    }

    public double Compute(ScheduledAssignmentCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAssignmentCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
