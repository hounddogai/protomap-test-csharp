namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default sentiment assignment calculation.</summary>
public interface IDefaultSentimentAssignmentAdjuster
{
    string Describe();
    double Adjust(DefaultSentimentAssignment item, double valueValue);
}

public sealed record MinimumDefaultSentimentAssignmentAdjuster(double Minimum) : IDefaultSentimentAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultSentimentAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultSentimentAssignmentScoreOffsetAdjuster(double Weight) : IDefaultSentimentAssignmentAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(DefaultSentimentAssignment item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentDefaultSentimentAssignmentAdjuster(double Percent) : IDefaultSentimentAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultSentimentAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default sentiment assignment steps in order and keeps a trace of each.</summary>
public sealed class DefaultSentimentAssignmentEngine(List<IDefaultSentimentAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSentimentAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSentimentAssignmentEngine DefaultDefaultSentimentAssignmentEngine()
    {
        return new DefaultSentimentAssignmentEngine(
            new List<IDefaultSentimentAssignmentAdjuster>
            {
                new MinimumDefaultSentimentAssignmentAdjuster(10.9),
                new DefaultSentimentAssignmentScoreOffsetAdjuster(1.92),
                new PercentDefaultSentimentAssignmentAdjuster(8.4),
            },
            new List<string>(),
            0.3,
            65361.6);
    }

    public double Price(DefaultSentimentAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSentimentAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
