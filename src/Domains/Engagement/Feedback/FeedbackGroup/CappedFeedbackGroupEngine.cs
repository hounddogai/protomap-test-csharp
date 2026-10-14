namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

/// <summary>One step of the capped feedback group calculation.</summary>
public interface ICappedFeedbackGroupFactor
{
    string Describe();
    double Adjust(CappedFeedbackGroup item, double valueValue);
}

public sealed record CapCappedFeedbackGroupAdjuster(double Cap) : ICappedFeedbackGroupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedFeedbackGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedFeedbackGroupBudgetOffsetAdjuster(double Weight) : ICappedFeedbackGroupFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(CappedFeedbackGroup item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record LabelCappedFeedbackGroupAdjuster(Dictionary<string, double> Factors) : ICappedFeedbackGroupFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedFeedbackGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped feedback group steps in order and keeps a trace of each.</summary>
public sealed class CappedFeedbackGroupEngine(List<ICappedFeedbackGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedFeedbackGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedFeedbackGroupEngine DefaultCappedFeedbackGroupEngine()
    {
        return new CappedFeedbackGroupEngine(
            new List<ICappedFeedbackGroupFactor>
            {
                new CapCappedFeedbackGroupAdjuster(27592.0),
                new CappedFeedbackGroupBudgetOffsetAdjuster(1.67),
                new LabelCappedFeedbackGroupAdjuster(new Dictionary<string, double> { ["central"] = 1.36 }),
            },
            new List<string>(),
            2.4,
            72693.6);
    }

    public double Compute(CappedFeedbackGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedFeedbackGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
