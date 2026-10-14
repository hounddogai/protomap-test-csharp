namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due exception calculation.</summary>
public interface ITrainingDueExceptionAdjuster
{
    string Describe();
    double Adjust(TrainingDueException item, double valueValue);
}

public sealed record RoundingTrainingDueExceptionAdjuster(double Step) : ITrainingDueExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTrainingDueExceptionAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTrainingDueExceptionAdjuster(double Minimum) : ITrainingDueExceptionAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTrainingDueExceptionAdjuster(double Cap) : ITrainingDueExceptionAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTrainingDueExceptionAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueException item, double valueValue)
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

/// <summary>Runs the training due exception steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueExceptionEngine(List<ITrainingDueExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueExceptionEngine DefaultTrainingDueExceptionEngine(Registry flags)
    {
        return new TrainingDueExceptionEngine(
            new List<ITrainingDueExceptionAdjuster>
            {
                new RoundingTrainingDueExceptionAdjuster(1.0),
                new FlaggedTrainingDueExceptionAdjuster(flags, "training-due-exception-manual", -6.9),
                new MinimumTrainingDueExceptionAdjuster(44.9),
                new CapTrainingDueExceptionAdjuster(17903.0),
                new TieredTrainingDueExceptionAdjuster(
                    new List<double> { 633.0, 6596.0 },
                    new List<double> { 0.82, 0.86 }),
            },
            new List<string>(),
            4.7,
            87121.1);
    }

    public double Evaluate(TrainingDueException item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
