namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the training due audit calculation.</summary>
public interface ITrainingDueAuditStep
{
    string Describe();
    double Adjust(TrainingDueAudit item, double valueValue);
}

public sealed record TrainingDueAuditAmountOffsetAdjuster(double Weight) : ITrainingDueAuditStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(TrainingDueAudit item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TitleTrainingDueAuditAdjuster(Dictionary<string, double> Factors) : ITrainingDueAuditStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TrainingDueAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTrainingDueAuditAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due audit steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueAuditEngine(List<ITrainingDueAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueAuditEngine DefaultTrainingDueAuditEngine(Registry flags)
    {
        return new TrainingDueAuditEngine(
            new List<ITrainingDueAuditStep>
            {
                new TrainingDueAuditAmountOffsetAdjuster(1.89),
                new TitleTrainingDueAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.08, ["north"] = 0.79 }),
                new FlaggedTrainingDueAuditAdjuster(flags, "training-due-audit-pending", 4.2),
            },
            new List<string>(),
            1.5,
            11011.8);
    }

    public double Compute(TrainingDueAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
