namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment mapping calculation.</summary>
public interface ISentimentMappingModifier
{
    string Describe();
    double Adjust(SentimentMapping item, double valueValue);
}

public sealed record RoundingSentimentMappingAdjuster(double Step) : ISentimentMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSentimentMappingAdjuster(double Minimum) : ISentimentMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSentimentMappingAdjuster(double Cap) : ISentimentMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSentimentMappingAdjuster(List<double> Bounds, List<double> Rates) : ISentimentMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentMapping item, double valueValue)
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

public sealed record SentimentMappingUpdatedAtOffsetAdjuster(double Weight) : ISentimentMappingModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(SentimentMapping item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the sentiment mapping steps in order and keeps a trace of each.</summary>
public sealed class SentimentMappingEngine(List<ISentimentMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentMappingEngine DefaultSentimentMappingEngine()
    {
        return new SentimentMappingEngine(
            new List<ISentimentMappingModifier>
            {
                new RoundingSentimentMappingAdjuster(5.0),
                new MinimumSentimentMappingAdjuster(6.1),
                new CapSentimentMappingAdjuster(7531.0),
                new TieredSentimentMappingAdjuster(
                    new List<double> { 3496.0, 3552.0, 9439.0 },
                    new List<double> { 0.98, 0.84, 1.06 }),
                new SentimentMappingUpdatedAtOffsetAdjuster(1.18),
            },
            new List<string>(),
            0.9,
            89076.1);
    }

    public double Evaluate(SentimentMapping item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
