namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling sentiment reservation calculation.</summary>
public interface IRollingSentimentReservationStep
{
    string Describe();
    double Adjust(RollingSentimentReservation item, double valueValue);
}

public sealed record CapRollingSentimentReservationAdjuster(double Cap) : IRollingSentimentReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingSentimentReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingSentimentReservationAdjuster(Registry Flags, string Flag, double Percent) : IRollingSentimentReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingSentimentReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRollingSentimentReservationAdjuster(Dictionary<string, double> Factors) : IRollingSentimentReservationStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingSentimentReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RollingSentimentReservationCeilingOffsetAdjuster(double Weight) : IRollingSentimentReservationStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RollingSentimentReservation item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record PercentRollingSentimentReservationAdjuster(double Percent) : IRollingSentimentReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSentimentReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling sentiment reservation steps in order and keeps a trace of each.</summary>
public sealed class RollingSentimentReservationEngine(List<IRollingSentimentReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSentimentReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSentimentReservationEngine DefaultRollingSentimentReservationEngine(Registry flags)
    {
        return new RollingSentimentReservationEngine(
            new List<IRollingSentimentReservationStep>
            {
                new CapRollingSentimentReservationAdjuster(31302.0),
                new FlaggedRollingSentimentReservationAdjuster(flags, "rolling-sentiment-reservation-regional", 1.8),
                new NameRollingSentimentReservationAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.8, ["retail"] = 0.93 }),
                new RollingSentimentReservationCeilingOffsetAdjuster(0.87),
                new PercentRollingSentimentReservationAdjuster(23.4),
            },
            new List<string>(),
            1.9,
            20200.7);
    }

    public double Compute(RollingSentimentReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingSentimentReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
