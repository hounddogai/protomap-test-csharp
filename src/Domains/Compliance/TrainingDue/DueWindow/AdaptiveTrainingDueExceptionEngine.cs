namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive training due exception calculation.</summary>
public interface IAdaptiveTrainingDueExceptionStep
{
    string Describe();
    double Adjust(AdaptiveTrainingDueException item, double valueValue);
}

public sealed record FlaggedAdaptiveTrainingDueExceptionAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTrainingDueExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTrainingDueException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAdaptiveTrainingDueExceptionAdjuster(Dictionary<string, double> Factors) : IAdaptiveTrainingDueExceptionStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveTrainingDueException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveTrainingDueExceptionAdjuster(double Cap) : IAdaptiveTrainingDueExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTrainingDueException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptiveTrainingDueExceptionAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveTrainingDueExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveTrainingDueException item, double valueValue)
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

/// <summary>Runs the adaptive training due exception steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTrainingDueExceptionEngine(List<IAdaptiveTrainingDueExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTrainingDueExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTrainingDueExceptionEngine DefaultAdaptiveTrainingDueExceptionEngine(Registry flags)
    {
        return new AdaptiveTrainingDueExceptionEngine(
            new List<IAdaptiveTrainingDueExceptionStep>
            {
                new FlaggedAdaptiveTrainingDueExceptionAdjuster(
                    flags,
                    "adaptive-training-due-exception-primary",
                    -0.9),
                new NameAdaptiveTrainingDueExceptionAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.02, ["online"] = 0.7 }),
                new CapAdaptiveTrainingDueExceptionAdjuster(14599.0),
                new TieredAdaptiveTrainingDueExceptionAdjuster(
                    new List<double> { 6780.0, 7157.0, 8723.0 },
                    new List<double> { 0.99, 0.91, 1.17 }),
            },
            new List<string>(),
            0.6,
            37002.9);
    }

    public double Compute(AdaptiveTrainingDueException item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTrainingDueException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
