namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback window calculation.</summary>
public interface IFeedbackWindowModifier
{
    string Describe();
    double Adjust(FeedbackWindow item, double valueValue);
}

public sealed record CapFeedbackWindowAdjuster(double Cap) : IFeedbackWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFeedbackWindowAdjuster(double Step) : IFeedbackWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FeedbackWindowFteOffsetAdjuster(double Weight) : IFeedbackWindowModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(FeedbackWindow item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the feedback window steps in order and keeps a trace of each.</summary>
public sealed class FeedbackWindowEngine(List<IFeedbackWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackWindowEngine DefaultFeedbackWindowEngine()
    {
        return new FeedbackWindowEngine(
            new List<IFeedbackWindowModifier>
            {
                new CapFeedbackWindowAdjuster(38295.0),
                new RoundingFeedbackWindowAdjuster(0.5),
                new FeedbackWindowFteOffsetAdjuster(1.58),
            },
            new List<string>(),
            2.7,
            32869.2);
    }

    public double Compute(FeedbackWindow item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
