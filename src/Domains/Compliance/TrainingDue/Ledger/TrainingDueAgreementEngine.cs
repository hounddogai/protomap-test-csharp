namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due agreement calculation.</summary>
public interface ITrainingDueAgreementStep
{
    string Describe();
    double Adjust(TrainingDueAgreement item, double valueValue);
}

public sealed record MinimumTrainingDueAgreementAdjuster(double Minimum) : ITrainingDueAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTrainingDueAgreementAdjuster(double Step) : ITrainingDueAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTrainingDueAgreementAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueAgreement item, double valueValue)
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

/// <summary>Runs the training due agreement steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueAgreementEngine(List<ITrainingDueAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueAgreementEngine DefaultTrainingDueAgreementEngine()
    {
        return new TrainingDueAgreementEngine(
            new List<ITrainingDueAgreementStep>
            {
                new MinimumTrainingDueAgreementAdjuster(17.7),
                new RoundingTrainingDueAgreementAdjuster(0.1),
                new TieredTrainingDueAgreementAdjuster(
                    new List<double> { 5257.0, 5646.0, 6023.0 },
                    new List<double> { 1.15, 0.9, 1.11 }),
            },
            new List<string>(),
            3.7,
            68741.9);
    }

    public double Evaluate(TrainingDueAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
