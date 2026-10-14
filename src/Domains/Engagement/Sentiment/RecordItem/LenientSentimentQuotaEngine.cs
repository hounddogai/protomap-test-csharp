namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient sentiment quota calculation.</summary>
public interface ILenientSentimentQuotaStep
{
    string Describe();
    double Adjust(LenientSentimentQuota item, double valueValue);
}

public sealed record MinimumLenientSentimentQuotaAdjuster(double Minimum) : ILenientSentimentQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientSentimentQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLenientSentimentQuotaAdjuster(double Step) : ILenientSentimentQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSentimentQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientSentimentQuotaAdjuster(Registry Flags, string Flag, double Percent) : ILenientSentimentQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSentimentQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientSentimentQuotaAdjuster(double Percent) : ILenientSentimentQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientSentimentQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLenientSentimentQuotaAdjuster(Dictionary<string, double> Factors) : ILenientSentimentQuotaStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientSentimentQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient sentiment quota steps in order and keeps a trace of each.</summary>
public sealed class LenientSentimentQuotaEngine(List<ILenientSentimentQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSentimentQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSentimentQuotaEngine DefaultLenientSentimentQuotaEngine(Registry flags)
    {
        return new LenientSentimentQuotaEngine(
            new List<ILenientSentimentQuotaStep>
            {
                new MinimumLenientSentimentQuotaAdjuster(1.4),
                new RoundingLenientSentimentQuotaAdjuster(0.25),
                new FlaggedLenientSentimentQuotaAdjuster(flags, "lenient-sentiment-quota-capped", -9.6),
                new PercentLenientSentimentQuotaAdjuster(-4.8),
                new TitleLenientSentimentQuotaAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.94, ["south"] = 0.71, ["retail"] = 0.78 }),
            },
            new List<string>(),
            3.8,
            79962.8);
    }

    public double Run(LenientSentimentQuota item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientSentimentQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
