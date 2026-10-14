namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due allocation calculation.</summary>
public interface ITrainingDueAllocationModifier
{
    string Describe();
    double Adjust(TrainingDueAllocation item, double valueValue);
}

public sealed record FlaggedTrainingDueAllocationAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueAllocationFteOffsetAdjuster(double Weight) : ITrainingDueAllocationModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(TrainingDueAllocation item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredTrainingDueAllocationAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueAllocation item, double valueValue)
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

public sealed record TitleTrainingDueAllocationAdjuster(Dictionary<string, double> Factors) : ITrainingDueAllocationModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TrainingDueAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the training due allocation steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueAllocationEngine(List<ITrainingDueAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueAllocationEngine DefaultTrainingDueAllocationEngine(Registry flags)
    {
        return new TrainingDueAllocationEngine(
            new List<ITrainingDueAllocationModifier>
            {
                new FlaggedTrainingDueAllocationAdjuster(flags, "training-due-allocation-adaptive", -3.3),
                new TrainingDueAllocationFteOffsetAdjuster(1.26),
                new TieredTrainingDueAllocationAdjuster(
                    new List<double> { 1834.0, 3893.0 },
                    new List<double> { 0.96, 1.11 }),
                new TitleTrainingDueAllocationAdjuster(new Dictionary<string, double> { ["central"] = 1.0 }),
            },
            new List<string>(),
            0.0,
            47552.1);
    }

    public double Compute(TrainingDueAllocation item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
