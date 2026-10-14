namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the assignment slot calculation.</summary>
public interface IAssignmentSlotStep
{
    string Describe();
    double Adjust(AssignmentSlot item, double valueValue);
}

public sealed record CapAssignmentSlotAdjuster(double Cap) : IAssignmentSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAssignmentSlotAdjuster(double Minimum) : IAssignmentSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelAssignmentSlotAdjuster(Dictionary<string, double> Factors) : IAssignmentSlotStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AssignmentSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AssignmentSlotCreatedAtOffsetAdjuster(double Weight) : IAssignmentSlotStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(AssignmentSlot item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record TieredAssignmentSlotAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentSlot item, double valueValue)
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

public sealed record FlaggedAssignmentSlotAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentSlotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment slot steps in order and keeps a trace of each.</summary>
public sealed class AssignmentSlotEngine(List<IAssignmentSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentSlotEngine DefaultAssignmentSlotEngine(Registry flags)
    {
        return new AssignmentSlotEngine(
            new List<IAssignmentSlotStep>
            {
                new CapAssignmentSlotAdjuster(30758.0),
                new MinimumAssignmentSlotAdjuster(24.8),
                new LabelAssignmentSlotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.9, ["north"] = 0.71, ["central"] = 0.76 }),
                new AssignmentSlotCreatedAtOffsetAdjuster(1.89),
                new TieredAssignmentSlotAdjuster(
                    new List<double> { 6642.0, 6684.0, 6914.0 },
                    new List<double> { 0.99, 1.2, 0.93 }),
                new FlaggedAssignmentSlotAdjuster(flags, "assignment-slot-batched", -1.5),
            },
            new List<string>(),
            3.4,
            64642.5);
    }

    public double Compute(AssignmentSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
