namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due claim calculation.</summary>
public interface ITrainingDueClaimStep
{
    string Describe();
    double Adjust(TrainingDueClaim item, double valueValue);
}

public sealed record TieredTrainingDueClaimAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueClaim item, double valueValue)
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

public sealed record MinimumTrainingDueClaimAdjuster(double Minimum) : ITrainingDueClaimStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTrainingDueClaimAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTrainingDueClaimAdjuster(double Cap) : ITrainingDueClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleTrainingDueClaimAdjuster(Dictionary<string, double> Factors) : ITrainingDueClaimStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TrainingDueClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTrainingDueClaimAdjuster(double Step) : ITrainingDueClaimStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the training due claim steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueClaimEngine(List<ITrainingDueClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueClaimEngine DefaultTrainingDueClaimEngine(Registry flags)
    {
        return new TrainingDueClaimEngine(
            new List<ITrainingDueClaimStep>
            {
                new TieredTrainingDueClaimAdjuster(
                    new List<double> { 3155.0, 5343.0, 6443.0, 7864.0 },
                    new List<double> { 0.81, 0.95, 1.1, 1.15 }),
                new MinimumTrainingDueClaimAdjuster(37.6),
                new FlaggedTrainingDueClaimAdjuster(flags, "training-due-claim-weighted", 2.2),
                new CapTrainingDueClaimAdjuster(21313.0),
                new TitleTrainingDueClaimAdjuster(new Dictionary<string, double> { ["south"] = 1.34 }),
                new RoundingTrainingDueClaimAdjuster(0.25),
            },
            new List<string>(),
            2.3,
            50645.1);
    }

    public double Evaluate(TrainingDueClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
