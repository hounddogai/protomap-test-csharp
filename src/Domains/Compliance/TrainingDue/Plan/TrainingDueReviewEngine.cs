namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due review calculation.</summary>
public interface ITrainingDueReviewFactor
{
    string Describe();
    double Adjust(TrainingDueReview item, double valueValue);
}

public sealed record MinimumTrainingDueReviewAdjuster(double Minimum) : ITrainingDueReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDueReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTrainingDueReviewAdjuster(double Cap) : ITrainingDueReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTrainingDueReviewAdjuster(double Percent) : ITrainingDueReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitTrainingDueReviewAdjuster(Dictionary<string, double> Factors) : ITrainingDueReviewFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(TrainingDueReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TrainingDueReviewLeaveDaysOffsetAdjuster(double Weight) : ITrainingDueReviewFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(TrainingDueReview item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the training due review steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueReviewEngine(List<ITrainingDueReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueReviewEngine DefaultTrainingDueReviewEngine()
    {
        return new TrainingDueReviewEngine(
            new List<ITrainingDueReviewFactor>
            {
                new MinimumTrainingDueReviewAdjuster(7.9),
                new CapTrainingDueReviewAdjuster(39296.0),
                new PercentTrainingDueReviewAdjuster(9.5),
                new UnitTrainingDueReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.32, ["east"] = 1.09, ["south"] = 0.96 }),
                new TrainingDueReviewLeaveDaysOffsetAdjuster(1.75),
            },
            new List<string>(),
            2.2,
            7571.6);
    }

    public double Compute(TrainingDueReview item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
