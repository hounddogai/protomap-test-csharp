namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched feedback score calculation.</summary>
public interface IBatchedFeedbackScoreAdjuster
{
    string Describe();
    double Adjust(BatchedFeedbackScore item, double valueValue);
}

public sealed record RoundingBatchedFeedbackScoreAdjuster(double Step) : IBatchedFeedbackScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedFeedbackScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBatchedFeedbackScoreAdjuster(double Cap) : IBatchedFeedbackScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedFeedbackScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BatchedFeedbackScoreHoursOffsetAdjuster(double Weight) : IBatchedFeedbackScoreAdjuster
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(BatchedFeedbackScore item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record TitleBatchedFeedbackScoreAdjuster(Dictionary<string, double> Factors) : IBatchedFeedbackScoreAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedFeedbackScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched feedback score steps in order and keeps a trace of each.</summary>
public sealed class BatchedFeedbackScoreEngine(List<IBatchedFeedbackScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedFeedbackScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedFeedbackScoreEngine DefaultBatchedFeedbackScoreEngine()
    {
        return new BatchedFeedbackScoreEngine(
            new List<IBatchedFeedbackScoreAdjuster>
            {
                new RoundingBatchedFeedbackScoreAdjuster(0.1),
                new CapBatchedFeedbackScoreAdjuster(8683.0),
                new BatchedFeedbackScoreHoursOffsetAdjuster(0.24),
                new TitleBatchedFeedbackScoreAdjuster(new Dictionary<string, double> { ["west"] = 0.74 }),
            },
            new List<string>(),
            4.0,
            48462.6);
    }

    public double Compute(BatchedFeedbackScore item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedFeedbackScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
