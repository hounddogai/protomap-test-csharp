namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment reservation calculation.</summary>
public interface ISentimentReservationFactor
{
    string Describe();
    double Adjust(SentimentReservation item, double valueValue);
}

public sealed record SentimentReservationThresholdOffsetAdjuster(double Weight) : ISentimentReservationFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(SentimentReservation item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record TieredSentimentReservationAdjuster(List<double> Bounds, List<double> Rates) : ISentimentReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentReservation item, double valueValue)
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

public sealed record PercentSentimentReservationAdjuster(double Percent) : ISentimentReservationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamSentimentReservationAdjuster(Dictionary<string, double> Factors) : ISentimentReservationFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SentimentReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment reservation steps in order and keeps a trace of each.</summary>
public sealed class SentimentReservationEngine(List<ISentimentReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentReservationEngine DefaultSentimentReservationEngine()
    {
        return new SentimentReservationEngine(
            new List<ISentimentReservationFactor>
            {
                new SentimentReservationThresholdOffsetAdjuster(0.15),
                new TieredSentimentReservationAdjuster(
                    new List<double> { 2413.0, 6308.0, 8215.0 },
                    new List<double> { 0.91, 1.18, 0.87 }),
                new PercentSentimentReservationAdjuster(10.1),
                new TeamSentimentReservationAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.99, ["north"] = 1.24 }),
            },
            new List<string>(),
            2.6,
            64170.5);
    }

    public double Evaluate(SentimentReservation item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
