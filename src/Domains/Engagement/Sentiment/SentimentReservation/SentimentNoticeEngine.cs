namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment notice calculation.</summary>
public interface ISentimentNoticeAdjuster
{
    string Describe();
    double Adjust(SentimentNotice item, double valueValue);
}

public sealed record CapSentimentNoticeAdjuster(double Cap) : ISentimentNoticeAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SentimentNoticeTenureMonthsOffsetAdjuster(double Weight) : ISentimentNoticeAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(SentimentNotice item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record ChannelSentimentNoticeAdjuster(Dictionary<string, double> Factors) : ISentimentNoticeAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(SentimentNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSentimentNoticeAdjuster(double Percent) : ISentimentNoticeAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment notice steps in order and keeps a trace of each.</summary>
public sealed class SentimentNoticeEngine(List<ISentimentNoticeAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentNoticeAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentNoticeEngine DefaultSentimentNoticeEngine()
    {
        return new SentimentNoticeEngine(
            new List<ISentimentNoticeAdjuster>
            {
                new CapSentimentNoticeAdjuster(29637.0),
                new SentimentNoticeTenureMonthsOffsetAdjuster(0.18),
                new ChannelSentimentNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.23, ["south"] = 1.23, ["west"] = 1.21 }),
                new PercentSentimentNoticeAdjuster(-22.0),
            },
            new List<string>(),
            1.5,
            21227.9);
    }

    public double Price(SentimentNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
