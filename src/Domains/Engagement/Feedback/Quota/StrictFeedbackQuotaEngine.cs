namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict feedback quota calculation.</summary>
public interface IStrictFeedbackQuotaAdjuster
{
    string Describe();
    double Adjust(StrictFeedbackQuota item, double valueValue);
}

public sealed record MinimumStrictFeedbackQuotaAdjuster(double Minimum) : IStrictFeedbackQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictFeedbackQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitStrictFeedbackQuotaAdjuster(Dictionary<string, double> Factors) : IStrictFeedbackQuotaAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(StrictFeedbackQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapStrictFeedbackQuotaAdjuster(double Cap) : IStrictFeedbackQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictFeedbackQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedStrictFeedbackQuotaAdjuster(Registry Flags, string Flag, double Percent) : IStrictFeedbackQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictFeedbackQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict feedback quota steps in order and keeps a trace of each.</summary>
public sealed class StrictFeedbackQuotaEngine(List<IStrictFeedbackQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictFeedbackQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictFeedbackQuotaEngine DefaultStrictFeedbackQuotaEngine(Registry flags)
    {
        return new StrictFeedbackQuotaEngine(
            new List<IStrictFeedbackQuotaAdjuster>
            {
                new MinimumStrictFeedbackQuotaAdjuster(35.9),
                new UnitStrictFeedbackQuotaAdjuster(new Dictionary<string, double> { ["west"] = 1.25 }),
                new CapStrictFeedbackQuotaAdjuster(26034.0),
                new FlaggedStrictFeedbackQuotaAdjuster(flags, "strict-feedback-quota-pending", 2.5),
            },
            new List<string>(),
            3.1,
            66534.5);
    }

    public double Evaluate(StrictFeedbackQuota item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictFeedbackQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
