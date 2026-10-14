namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled exception assignment calculation.</summary>
public interface IScheduledExceptionAssignmentStep
{
    string Describe();
    double Adjust(ScheduledExceptionAssignment item, double valueValue);
}

public sealed record SegmentScheduledExceptionAssignmentAdjuster(Dictionary<string, double> Factors) : IScheduledExceptionAssignmentStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ScheduledExceptionAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScheduledExceptionAssignmentAdjuster(double Percent) : IScheduledExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledExceptionAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScheduledExceptionAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IScheduledExceptionAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledExceptionAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledExceptionAssignmentAdjuster(double Cap) : IScheduledExceptionAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledExceptionAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledExceptionAssignmentAdjuster(double Step) : IScheduledExceptionAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledExceptionAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledExceptionAssignmentAdjuster(double Minimum) : IScheduledExceptionAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledExceptionAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled exception assignment steps in order and keeps a trace of each.</summary>
public sealed class ScheduledExceptionAssignmentEngine(List<IScheduledExceptionAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledExceptionAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledExceptionAssignmentEngine DefaultScheduledExceptionAssignmentEngine(Registry flags)
    {
        return new ScheduledExceptionAssignmentEngine(
            new List<IScheduledExceptionAssignmentStep>
            {
                new SegmentScheduledExceptionAssignmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.31, ["south"] = 1.25, ["retail"] = 0.9 }),
                new PercentScheduledExceptionAssignmentAdjuster(-5.7),
                new FlaggedScheduledExceptionAssignmentAdjuster(flags, "scheduled-exception-assignment-lenient", 7.1),
                new CapScheduledExceptionAssignmentAdjuster(49198.0),
                new RoundingScheduledExceptionAssignmentAdjuster(0.1),
                new MinimumScheduledExceptionAssignmentAdjuster(12.0),
            },
            new List<string>(),
            3.9,
            59781.9);
    }

    public double Run(ScheduledExceptionAssignment item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledExceptionAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
