namespace Acme.Workforce.Compliance.TrainingDue.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted training due assignment calculation.</summary>
public interface IWeightedTrainingDueAssignmentAdjuster
{
    string Describe();
    double Adjust(WeightedTrainingDueAssignment item, double valueValue);
}

public sealed record TieredWeightedTrainingDueAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IWeightedTrainingDueAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedTrainingDueAssignment item, double valueValue)
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

public sealed record RoundingWeightedTrainingDueAssignmentAdjuster(double Step) : IWeightedTrainingDueAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedTrainingDueAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamWeightedTrainingDueAssignmentAdjuster(Dictionary<string, double> Factors) : IWeightedTrainingDueAssignmentAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(WeightedTrainingDueAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedTrainingDueAssignmentAdjuster(double Minimum) : IWeightedTrainingDueAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedTrainingDueAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedTrainingDueAssignmentAdjuster(double Percent) : IWeightedTrainingDueAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedTrainingDueAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedTrainingDueAssignmentAdjuster(double Cap) : IWeightedTrainingDueAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedTrainingDueAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted training due assignment steps in order and keeps a trace of each.</summary>
public sealed class WeightedTrainingDueAssignmentEngine(List<IWeightedTrainingDueAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedTrainingDueAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedTrainingDueAssignmentEngine DefaultWeightedTrainingDueAssignmentEngine()
    {
        return new WeightedTrainingDueAssignmentEngine(
            new List<IWeightedTrainingDueAssignmentAdjuster>
            {
                new TieredWeightedTrainingDueAssignmentAdjuster(
                    new List<double> { 1970.0, 3056.0, 3100.0, 6159.0 },
                    new List<double> { 0.95, 0.93, 1.19, 1.16 }),
                new RoundingWeightedTrainingDueAssignmentAdjuster(0.5),
                new TeamWeightedTrainingDueAssignmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.26, ["online"] = 0.84 }),
                new MinimumWeightedTrainingDueAssignmentAdjuster(0.4),
                new PercentWeightedTrainingDueAssignmentAdjuster(-7.4),
                new CapWeightedTrainingDueAssignmentAdjuster(5523.0),
            },
            new List<string>(),
            4.4,
            92123.5);
    }

    public double Evaluate(WeightedTrainingDueAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedTrainingDueAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
