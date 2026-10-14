namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due rollup calculation.</summary>
public interface ITrainingDueRollupModifier
{
    string Describe();
    double Adjust(TrainingDueRollup item, double valueValue);
}

public sealed record LabelTrainingDueRollupAdjuster(Dictionary<string, double> Factors) : ITrainingDueRollupModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TrainingDueRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTrainingDueRollupAdjuster(double Step) : ITrainingDueRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTrainingDueRollupAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTrainingDueRollupAdjuster(double Cap) : ITrainingDueRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the training due rollup steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueRollupEngine(List<ITrainingDueRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueRollupEngine DefaultTrainingDueRollupEngine(Registry flags)
    {
        return new TrainingDueRollupEngine(
            new List<ITrainingDueRollupModifier>
            {
                new LabelTrainingDueRollupAdjuster(new Dictionary<string, double> { ["retail"] = 1.06 }),
                new RoundingTrainingDueRollupAdjuster(0.5),
                new FlaggedTrainingDueRollupAdjuster(flags, "training-due-rollup-scheduled", -5.9),
                new CapTrainingDueRollupAdjuster(11022.0),
            },
            new List<string>(),
            3.4,
            60301.5);
    }

    public double Evaluate(TrainingDueRollup item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
