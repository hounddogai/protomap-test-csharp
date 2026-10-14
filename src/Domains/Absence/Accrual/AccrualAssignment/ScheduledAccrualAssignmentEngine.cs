namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled accrual assignment calculation.</summary>
public interface IScheduledAccrualAssignmentAdjuster
{
    string Describe();
    double Adjust(ScheduledAccrualAssignment item, double valueValue);
}

public sealed record LabelScheduledAccrualAssignmentAdjuster(Dictionary<string, double> Factors) : IScheduledAccrualAssignmentAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledAccrualAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScheduledAccrualAssignmentAdjuster(double Percent) : IScheduledAccrualAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAccrualAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedScheduledAccrualAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAccrualAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAccrualAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledAccrualAssignmentAdjuster(double Minimum) : IScheduledAccrualAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAccrualAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScheduledAccrualAssignmentAdjuster(double Step) : IScheduledAccrualAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAccrualAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled accrual assignment steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAccrualAssignmentEngine(List<IScheduledAccrualAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAccrualAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAccrualAssignmentEngine DefaultScheduledAccrualAssignmentEngine(Registry flags)
    {
        return new ScheduledAccrualAssignmentEngine(
            new List<IScheduledAccrualAssignmentAdjuster>
            {
                new LabelScheduledAccrualAssignmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["retail"] = 1.31, ["west"] = 1.14 }),
                new PercentScheduledAccrualAssignmentAdjuster(-0.5),
                new FlaggedScheduledAccrualAssignmentAdjuster(flags, "scheduled-accrual-assignment-default", -7.3),
                new MinimumScheduledAccrualAssignmentAdjuster(44.0),
                new RoundingScheduledAccrualAssignmentAdjuster(0.5),
            },
            new List<string>(),
            3.5,
            27645.4);
    }

    public double Evaluate(ScheduledAccrualAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAccrualAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
