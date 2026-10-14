namespace Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>One step of the fallback feedback entry calculation.</summary>
public interface IFallbackFeedbackEntryAdjuster
{
    string Describe();
    double Adjust(FallbackFeedbackEntry item, double valueValue);
}

public sealed record TeamFallbackFeedbackEntryAdjuster(Dictionary<string, double> Factors) : IFallbackFeedbackEntryAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(FallbackFeedbackEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFallbackFeedbackEntryAdjuster(List<double> Bounds, List<double> Rates) : IFallbackFeedbackEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackFeedbackEntry item, double valueValue)
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

public sealed record CapFallbackFeedbackEntryAdjuster(double Cap) : IFallbackFeedbackEntryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackFeedbackEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackFeedbackEntryLeaveDaysOffsetAdjuster(double Weight) : IFallbackFeedbackEntryAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(FallbackFeedbackEntry item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the fallback feedback entry steps in order and keeps a trace of each.</summary>
public sealed class FallbackFeedbackEntryEngine(List<IFallbackFeedbackEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackFeedbackEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackFeedbackEntryEngine DefaultFallbackFeedbackEntryEngine()
    {
        return new FallbackFeedbackEntryEngine(
            new List<IFallbackFeedbackEntryAdjuster>
            {
                new TeamFallbackFeedbackEntryAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.38, ["central"] = 1.2, ["online"] = 1.24 }),
                new TieredFallbackFeedbackEntryAdjuster(
                    new List<double> { 5375.0, 8748.0 },
                    new List<double> { 0.87, 0.92 }),
                new CapFallbackFeedbackEntryAdjuster(40651.0),
                new FallbackFeedbackEntryLeaveDaysOffsetAdjuster(1.38),
            },
            new List<string>(),
            1.7,
            50982.9);
    }

    public double Compute(FallbackFeedbackEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackFeedbackEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
