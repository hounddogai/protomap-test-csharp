namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment adjustment calculation.</summary>
public interface IAssignmentAdjustmentStep
{
    string Describe();
    double Adjust(AssignmentAdjustment item, double valueValue);
}

public sealed record FlaggedAssignmentAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAssignmentAdjustmentAdjuster(double Step) : IAssignmentAdjustmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameAssignmentAdjustmentAdjuster(Dictionary<string, double> Factors) : IAssignmentAdjustmentStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AssignmentAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAssignmentAdjustmentAdjuster(double Minimum) : IAssignmentAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AssignmentAdjustmentDurationMinutesOffsetAdjuster(double Weight) : IAssignmentAdjustmentStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(AssignmentAdjustment item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record CapAssignmentAdjustmentAdjuster(double Cap) : IAssignmentAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the assignment adjustment steps in order and keeps a trace of each.</summary>
public sealed class AssignmentAdjustmentEngine(List<IAssignmentAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentAdjustmentEngine DefaultAssignmentAdjustmentEngine(Registry flags)
    {
        return new AssignmentAdjustmentEngine(
            new List<IAssignmentAdjustmentStep>
            {
                new FlaggedAssignmentAdjustmentAdjuster(flags, "assignment-adjustment-capped", -6.2),
                new RoundingAssignmentAdjustmentAdjuster(0.5),
                new NameAssignmentAdjustmentAdjuster(new Dictionary<string, double> { ["east"] = 1.18 }),
                new MinimumAssignmentAdjustmentAdjuster(44.9),
                new AssignmentAdjustmentDurationMinutesOffsetAdjuster(1.91),
                new CapAssignmentAdjustmentAdjuster(49168.0),
            },
            new List<string>(),
            1.4,
            29908.2);
    }

    public double Price(AssignmentAdjustment item)
    {
        var current = item.ProjectedValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AssignmentAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
