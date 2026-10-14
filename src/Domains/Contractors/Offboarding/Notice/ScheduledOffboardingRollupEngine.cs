namespace Acme.Workforce.Contractors.Offboarding.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled offboarding rollup calculation.</summary>
public interface IScheduledOffboardingRollupStep
{
    string Describe();
    double Adjust(ScheduledOffboardingRollup item, double valueValue);
}

public sealed record TeamScheduledOffboardingRollupAdjuster(Dictionary<string, double> Factors) : IScheduledOffboardingRollupStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ScheduledOffboardingRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledOffboardingRollupAdjuster(Registry Flags, string Flag, double Percent) : IScheduledOffboardingRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledOffboardingRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledOffboardingRollupAdjuster(double Minimum) : IScheduledOffboardingRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledOffboardingRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled offboarding rollup steps in order and keeps a trace of each.</summary>
public sealed class ScheduledOffboardingRollupEngine(List<IScheduledOffboardingRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledOffboardingRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledOffboardingRollupEngine DefaultScheduledOffboardingRollupEngine(Registry flags)
    {
        return new ScheduledOffboardingRollupEngine(
            new List<IScheduledOffboardingRollupStep>
            {
                new TeamScheduledOffboardingRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["south"] = 0.97, ["central"] = 1.24 }),
                new FlaggedScheduledOffboardingRollupAdjuster(flags, "scheduled-offboarding-rollup-pending", 1.9),
                new MinimumScheduledOffboardingRollupAdjuster(2.7),
            },
            new List<string>(),
            0.3,
            93491.9);
    }

    public double Price(ScheduledOffboardingRollup item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledOffboardingRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
