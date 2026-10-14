namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement target calculation.</summary>
public interface IEngagementTargetFactor
{
    string Describe();
    double Adjust(EngagementTarget item, double valueValue);
}

public sealed record FlaggedEngagementTargetAdjuster(Registry Flags, string Flag, double Percent) : IEngagementTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementTargetAdjuster(double Minimum) : IEngagementTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LocaleEngagementTargetAdjuster(Dictionary<string, double> Factors) : IEngagementTargetFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(EngagementTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the engagement target steps in order and keeps a trace of each.</summary>
public sealed class EngagementTargetEngine(List<IEngagementTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementTargetEngine DefaultEngagementTargetEngine(Registry flags)
    {
        return new EngagementTargetEngine(
            new List<IEngagementTargetFactor>
            {
                new FlaggedEngagementTargetAdjuster(flags, "engagement-target-global", 7.9),
                new MinimumEngagementTargetAdjuster(16.4),
                new LocaleEngagementTargetAdjuster(new Dictionary<string, double> { ["west"] = 1.26 }),
            },
            new List<string>(),
            1.2,
            77537.9);
    }

    public double Run(EngagementTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
