namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary training due batch calculation.</summary>
public interface IPrimaryTrainingDueBatchAdjuster
{
    string Describe();
    double Adjust(PrimaryTrainingDueBatch item, double valueValue);
}

public sealed record RoundingPrimaryTrainingDueBatchAdjuster(double Step) : IPrimaryTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryTrainingDueBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamPrimaryTrainingDueBatchAdjuster(Dictionary<string, double> Factors) : IPrimaryTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(PrimaryTrainingDueBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPrimaryTrainingDueBatchAdjuster(double Cap) : IPrimaryTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryTrainingDueBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPrimaryTrainingDueBatchAdjuster(double Minimum) : IPrimaryTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryTrainingDueBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryTrainingDueBatchRatingOffsetAdjuster(double Weight) : IPrimaryTrainingDueBatchAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(PrimaryTrainingDueBatch item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the primary training due batch steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTrainingDueBatchEngine(List<IPrimaryTrainingDueBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTrainingDueBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTrainingDueBatchEngine DefaultPrimaryTrainingDueBatchEngine()
    {
        return new PrimaryTrainingDueBatchEngine(
            new List<IPrimaryTrainingDueBatchAdjuster>
            {
                new RoundingPrimaryTrainingDueBatchAdjuster(1.0),
                new TeamPrimaryTrainingDueBatchAdjuster(new Dictionary<string, double> { ["west"] = 1.37 }),
                new CapPrimaryTrainingDueBatchAdjuster(7223.0),
                new MinimumPrimaryTrainingDueBatchAdjuster(35.6),
                new PrimaryTrainingDueBatchRatingOffsetAdjuster(0.27),
            },
            new List<string>(),
            4.0,
            11119.3);
    }

    public double Price(PrimaryTrainingDueBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTrainingDueBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
