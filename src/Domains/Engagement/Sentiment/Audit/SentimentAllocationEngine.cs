namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment allocation calculation.</summary>
public interface ISentimentAllocationStep
{
    string Describe();
    double Adjust(SentimentAllocation item, double valueValue);
}

public sealed record PercentSentimentAllocationAdjuster(double Percent) : ISentimentAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSentimentAllocationAdjuster(Registry Flags, string Flag, double Percent) : ISentimentAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSentimentAllocationAdjuster(List<double> Bounds, List<double> Rates) : ISentimentAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentAllocation item, double valueValue)
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

public sealed record CapSentimentAllocationAdjuster(double Cap) : ISentimentAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSentimentAllocationAdjuster(double Minimum) : ISentimentAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameSentimentAllocationAdjuster(Dictionary<string, double> Factors) : ISentimentAllocationStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SentimentAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment allocation steps in order and keeps a trace of each.</summary>
public sealed class SentimentAllocationEngine(List<ISentimentAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentAllocationEngine DefaultSentimentAllocationEngine(Registry flags)
    {
        return new SentimentAllocationEngine(
            new List<ISentimentAllocationStep>
            {
                new PercentSentimentAllocationAdjuster(-15.5),
                new FlaggedSentimentAllocationAdjuster(flags, "sentiment-allocation-manual", -5.0),
                new TieredSentimentAllocationAdjuster(
                    new List<double> { 77.0, 2400.0, 3056.0, 3641.0 },
                    new List<double> { 1.15, 0.85, 1.09, 0.9 }),
                new CapSentimentAllocationAdjuster(4861.0),
                new MinimumSentimentAllocationAdjuster(36.5),
                new NameSentimentAllocationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.93, ["west"] = 1.39 }),
            },
            new List<string>(),
            0.3,
            17306.4);
    }

    public double Evaluate(SentimentAllocation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
