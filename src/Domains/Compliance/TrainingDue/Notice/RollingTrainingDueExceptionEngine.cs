namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling training due exception calculation.</summary>
public interface IRollingTrainingDueExceptionStep
{
    string Describe();
    double Adjust(RollingTrainingDueException item, double valueValue);
}

public sealed record NameRollingTrainingDueExceptionAdjuster(Dictionary<string, double> Factors) : IRollingTrainingDueExceptionStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingTrainingDueException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingTrainingDueExceptionAdjuster(double Percent) : IRollingTrainingDueExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingTrainingDueException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingTrainingDueExceptionAdjuster(double Step) : IRollingTrainingDueExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingTrainingDueException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling training due exception steps in order and keeps a trace of each.</summary>
public sealed class RollingTrainingDueExceptionEngine(List<IRollingTrainingDueExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingTrainingDueExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingTrainingDueExceptionEngine DefaultRollingTrainingDueExceptionEngine()
    {
        return new RollingTrainingDueExceptionEngine(
            new List<IRollingTrainingDueExceptionStep>
            {
                new NameRollingTrainingDueExceptionAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.98, ["east"] = 1.16 }),
                new PercentRollingTrainingDueExceptionAdjuster(9.6),
                new RoundingRollingTrainingDueExceptionAdjuster(0.5),
            },
            new List<string>(),
            4.3,
            33606.6);
    }

    public double Run(RollingTrainingDueException item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingTrainingDueException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
