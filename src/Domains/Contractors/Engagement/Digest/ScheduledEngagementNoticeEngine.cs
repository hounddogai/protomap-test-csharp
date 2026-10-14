namespace Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>One step of the scheduled engagement notice calculation.</summary>
public interface IScheduledEngagementNoticeFactor
{
    string Describe();
    double Adjust(ScheduledEngagementNotice item, double valueValue);
}

public sealed record PercentScheduledEngagementNoticeAdjuster(double Percent) : IScheduledEngagementNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledEngagementNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledEngagementNoticeCeilingOffsetAdjuster(double Weight) : IScheduledEngagementNoticeFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(ScheduledEngagementNotice item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record NameScheduledEngagementNoticeAdjuster(Dictionary<string, double> Factors) : IScheduledEngagementNoticeFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledEngagementNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScheduledEngagementNoticeAdjuster(List<double> Bounds, List<double> Rates) : IScheduledEngagementNoticeFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledEngagementNotice item, double valueValue)
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

/// <summary>Runs the scheduled engagement notice steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEngagementNoticeEngine(List<IScheduledEngagementNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEngagementNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEngagementNoticeEngine DefaultScheduledEngagementNoticeEngine()
    {
        return new ScheduledEngagementNoticeEngine(
            new List<IScheduledEngagementNoticeFactor>
            {
                new PercentScheduledEngagementNoticeAdjuster(11.8),
                new ScheduledEngagementNoticeCeilingOffsetAdjuster(1.54),
                new NameScheduledEngagementNoticeAdjuster(new Dictionary<string, double> { ["north"] = 0.99 }),
                new TieredScheduledEngagementNoticeAdjuster(
                    new List<double> { 3231.0, 9451.0 },
                    new List<double> { 0.94, 1.11 }),
            },
            new List<string>(),
            0.0,
            68752.6);
    }

    public double Evaluate(ScheduledEngagementNotice item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEngagementNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
