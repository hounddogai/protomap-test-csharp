namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment profile calculation.</summary>
public interface ISentimentProfileModifier
{
    string Describe();
    double Adjust(SentimentProfile item, double valueValue);
}

public sealed record PercentSentimentProfileAdjuster(double Percent) : ISentimentProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSentimentProfileAdjuster(double Minimum) : ISentimentProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SentimentProfileLeaveDaysOffsetAdjuster(double Weight) : ISentimentProfileModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(SentimentProfile item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record RoundingSentimentProfileAdjuster(double Step) : ISentimentProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSentimentProfileAdjuster(double Cap) : ISentimentProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the sentiment profile steps in order and keeps a trace of each.</summary>
public sealed class SentimentProfileEngine(List<ISentimentProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentProfileEngine DefaultSentimentProfileEngine()
    {
        return new SentimentProfileEngine(
            new List<ISentimentProfileModifier>
            {
                new PercentSentimentProfileAdjuster(-10.4),
                new MinimumSentimentProfileAdjuster(12.7),
                new SentimentProfileLeaveDaysOffsetAdjuster(1.11),
                new RoundingSentimentProfileAdjuster(1.0),
                new CapSentimentProfileAdjuster(35615.0),
            },
            new List<string>(),
            1.7,
            87752.8);
    }

    public double Run(SentimentProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
