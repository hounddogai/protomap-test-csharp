namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan assignment calculation.</summary>
public interface ISuccessionPlanAssignmentAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanAssignment item, double valueValue);
}

public sealed record FlaggedSuccessionPlanAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSuccessionPlanAssignmentAdjuster(double Minimum) : ISuccessionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSuccessionPlanAssignmentAdjuster(double Step) : ISuccessionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SuccessionPlanAssignmentFloorValueOffsetAdjuster(double Weight) : ISuccessionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(SuccessionPlanAssignment item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentSuccessionPlanAssignmentAdjuster(double Percent) : ISuccessionPlanAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the succession plan assignment steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanAssignmentEngine(List<ISuccessionPlanAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanAssignmentEngine DefaultSuccessionPlanAssignmentEngine(Registry flags)
    {
        return new SuccessionPlanAssignmentEngine(
            new List<ISuccessionPlanAssignmentAdjuster>
            {
                new FlaggedSuccessionPlanAssignmentAdjuster(flags, "succession-plan-assignment-adaptive", 8.4),
                new MinimumSuccessionPlanAssignmentAdjuster(49.0),
                new RoundingSuccessionPlanAssignmentAdjuster(0.25),
                new SuccessionPlanAssignmentFloorValueOffsetAdjuster(0.53),
                new PercentSuccessionPlanAssignmentAdjuster(15.5),
            },
            new List<string>(),
            4.3,
            39612.5);
    }

    public double Compute(SuccessionPlanAssignment item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
