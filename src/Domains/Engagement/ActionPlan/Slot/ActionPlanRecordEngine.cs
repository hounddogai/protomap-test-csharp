namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan record calculation.</summary>
public interface IActionPlanRecordStep
{
    string Describe();
    double Adjust(ActionPlanRecord item, double valueValue);
}

public sealed record RoundingActionPlanRecordAdjuster(double Step) : IActionPlanRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumActionPlanRecordAdjuster(double Minimum) : IActionPlanRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedActionPlanRecordAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapActionPlanRecordAdjuster(double Cap) : IActionPlanRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ActionPlanRecordBatchSizeOffsetAdjuster(double Weight) : IActionPlanRecordStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ActionPlanRecord item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentActionPlanRecordAdjuster(double Percent) : IActionPlanRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan record steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanRecordEngine(List<IActionPlanRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanRecordEngine DefaultActionPlanRecordEngine(Registry flags)
    {
        return new ActionPlanRecordEngine(
            new List<IActionPlanRecordStep>
            {
                new RoundingActionPlanRecordAdjuster(0.25),
                new MinimumActionPlanRecordAdjuster(2.1),
                new FlaggedActionPlanRecordAdjuster(flags, "action-plan-record-scheduled", 0.4),
                new CapActionPlanRecordAdjuster(15860.0),
                new ActionPlanRecordBatchSizeOffsetAdjuster(0.79),
                new PercentActionPlanRecordAdjuster(-2.9),
            },
            new List<string>(),
            1.3,
            49472.7);
    }

    public double Run(ActionPlanRecord item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
