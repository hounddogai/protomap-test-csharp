namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending sentiment batch calculation.</summary>
public interface IPendingSentimentBatchAdjuster
{
    string Describe();
    double Adjust(PendingSentimentBatch item, double valueValue);
}

public sealed record NamePendingSentimentBatchAdjuster(Dictionary<string, double> Factors) : IPendingSentimentBatchAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingSentimentBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPendingSentimentBatchAdjuster(double Cap) : IPendingSentimentBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingSentimentBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPendingSentimentBatchAdjuster(double Minimum) : IPendingSentimentBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingSentimentBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPendingSentimentBatchAdjuster(double Step) : IPendingSentimentBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingSentimentBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPendingSentimentBatchAdjuster(List<double> Bounds, List<double> Rates) : IPendingSentimentBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingSentimentBatch item, double valueValue)
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

public sealed record PercentPendingSentimentBatchAdjuster(double Percent) : IPendingSentimentBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingSentimentBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending sentiment batch steps in order and keeps a trace of each.</summary>
public sealed class PendingSentimentBatchEngine(List<IPendingSentimentBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSentimentBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSentimentBatchEngine DefaultPendingSentimentBatchEngine()
    {
        return new PendingSentimentBatchEngine(
            new List<IPendingSentimentBatchAdjuster>
            {
                new NamePendingSentimentBatchAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.73, ["east"] = 0.77 }),
                new CapPendingSentimentBatchAdjuster(39467.0),
                new MinimumPendingSentimentBatchAdjuster(48.3),
                new RoundingPendingSentimentBatchAdjuster(0.05),
                new TieredPendingSentimentBatchAdjuster(
                    new List<double> { 3534.0, 9514.0 },
                    new List<double> { 0.86, 0.88 }),
                new PercentPendingSentimentBatchAdjuster(24.7),
            },
            new List<string>(),
            4.0,
            92901.2);
    }

    public double Run(PendingSentimentBatch item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PendingSentimentBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
