namespace Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>One step of the rolling engagement digest calculation.</summary>
public interface IRollingEngagementDigestAdjuster
{
    string Describe();
    double Adjust(RollingEngagementDigest item, double valueValue);
}

public sealed record PercentRollingEngagementDigestAdjuster(double Percent) : IRollingEngagementDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingEngagementDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingEngagementDigestAdjuster(double Minimum) : IRollingEngagementDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingEngagementDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelRollingEngagementDigestAdjuster(Dictionary<string, double> Factors) : IRollingEngagementDigestAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RollingEngagementDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling engagement digest steps in order and keeps a trace of each.</summary>
public sealed class RollingEngagementDigestEngine(List<IRollingEngagementDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingEngagementDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingEngagementDigestEngine DefaultRollingEngagementDigestEngine()
    {
        return new RollingEngagementDigestEngine(
            new List<IRollingEngagementDigestAdjuster>
            {
                new PercentRollingEngagementDigestAdjuster(10.6),
                new MinimumRollingEngagementDigestAdjuster(8.5),
                new LabelRollingEngagementDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.96, ["north"] = 0.71, ["south"] = 1.3 }),
            },
            new List<string>(),
            3.3,
            38343.1);
    }

    public double Evaluate(RollingEngagementDigest item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingEngagementDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
