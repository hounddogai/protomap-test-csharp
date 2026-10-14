namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the training due budget calculation.</summary>
public interface ITrainingDueBudgetModifier
{
    string Describe();
    double Adjust(TrainingDueBudget item, double valueValue);
}

public sealed record PercentTrainingDueBudgetAdjuster(double Percent) : ITrainingDueBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TrainingDueBudgetFteOffsetAdjuster(double Weight) : ITrainingDueBudgetModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(TrainingDueBudget item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record FlaggedTrainingDueBudgetAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleTrainingDueBudgetAdjuster(Dictionary<string, double> Factors) : ITrainingDueBudgetModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(TrainingDueBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTrainingDueBudgetAdjuster(double Cap) : ITrainingDueBudgetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the training due budget steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueBudgetEngine(List<ITrainingDueBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueBudgetEngine DefaultTrainingDueBudgetEngine(Registry flags)
    {
        return new TrainingDueBudgetEngine(
            new List<ITrainingDueBudgetModifier>
            {
                new PercentTrainingDueBudgetAdjuster(20.0),
                new TrainingDueBudgetFteOffsetAdjuster(0.32),
                new FlaggedTrainingDueBudgetAdjuster(flags, "training-due-budget-tiered", 2.8),
                new LocaleTrainingDueBudgetAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.02, ["central"] = 0.86 }),
                new CapTrainingDueBudgetAdjuster(39389.0),
            },
            new List<string>(),
            0.0,
            92131.5);
    }

    public double Price(TrainingDueBudget item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
