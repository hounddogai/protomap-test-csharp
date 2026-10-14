namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default feedback target calculation.</summary>
public interface IDefaultFeedbackTargetModifier
{
    string Describe();
    double Adjust(DefaultFeedbackTarget item, double valueValue);
}

public sealed record CapDefaultFeedbackTargetAdjuster(double Cap) : IDefaultFeedbackTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultFeedbackTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDefaultFeedbackTargetAdjuster(double Step) : IDefaultFeedbackTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultFeedbackTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleDefaultFeedbackTargetAdjuster(Dictionary<string, double> Factors) : IDefaultFeedbackTargetModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultFeedbackTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultFeedbackTargetAdjuster(double Minimum) : IDefaultFeedbackTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultFeedbackTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default feedback target steps in order and keeps a trace of each.</summary>
public sealed class DefaultFeedbackTargetEngine(List<IDefaultFeedbackTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultFeedbackTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultFeedbackTargetEngine DefaultDefaultFeedbackTargetEngine()
    {
        return new DefaultFeedbackTargetEngine(
            new List<IDefaultFeedbackTargetModifier>
            {
                new CapDefaultFeedbackTargetAdjuster(25607.0),
                new RoundingDefaultFeedbackTargetAdjuster(1.0),
                new TitleDefaultFeedbackTargetAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.91, ["retail"] = 0.93, ["west"] = 0.75 }),
                new MinimumDefaultFeedbackTargetAdjuster(6.4),
            },
            new List<string>(),
            0.1,
            74371.5);
    }

    public double Run(DefaultFeedbackTarget item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultFeedbackTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
