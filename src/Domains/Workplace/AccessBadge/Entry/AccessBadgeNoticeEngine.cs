namespace Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>One step of the access badge notice calculation.</summary>
public interface IAccessBadgeNoticeFactor
{
    string Describe();
    double Adjust(AccessBadgeNotice item, double valueValue);
}

public sealed record TitleAccessBadgeNoticeAdjuster(Dictionary<string, double> Factors) : IAccessBadgeNoticeFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AccessBadgeNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAccessBadgeNoticeAdjuster(double Percent) : IAccessBadgeNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAccessBadgeNoticeAdjuster(double Cap) : IAccessBadgeNoticeFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the access badge notice steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeNoticeEngine(List<IAccessBadgeNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeNoticeEngine DefaultAccessBadgeNoticeEngine()
    {
        return new AccessBadgeNoticeEngine(
            new List<IAccessBadgeNoticeFactor>
            {
                new TitleAccessBadgeNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.22, ["north"] = 0.95, ["retail"] = 1.29 }),
                new PercentAccessBadgeNoticeAdjuster(-8.2),
                new CapAccessBadgeNoticeAdjuster(5063.0),
            },
            new List<string>(),
            3.9,
            5425.8);
    }

    public double Evaluate(AccessBadgeNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
