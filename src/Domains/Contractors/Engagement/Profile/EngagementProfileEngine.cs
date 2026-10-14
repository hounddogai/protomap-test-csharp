namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement profile calculation.</summary>
public interface IEngagementProfileStep
{
    string Describe();
    double Adjust(EngagementProfile item, double valueValue);
}

public sealed record EngagementProfileScoreOffsetAdjuster(double Weight) : IEngagementProfileStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(EngagementProfile item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record FlaggedEngagementProfileAdjuster(Registry Flags, string Flag, double Percent) : IEngagementProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementProfileAdjuster(double Minimum) : IEngagementProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapEngagementProfileAdjuster(double Cap) : IEngagementProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentEngagementProfileAdjuster(double Percent) : IEngagementProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the engagement profile steps in order and keeps a trace of each.</summary>
public sealed class EngagementProfileEngine(List<IEngagementProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementProfileEngine DefaultEngagementProfileEngine(Registry flags)
    {
        return new EngagementProfileEngine(
            new List<IEngagementProfileStep>
            {
                new EngagementProfileScoreOffsetAdjuster(0.72),
                new FlaggedEngagementProfileAdjuster(flags, "engagement-profile-primary", -9.7),
                new MinimumEngagementProfileAdjuster(24.3),
                new CapEngagementProfileAdjuster(39975.0),
                new PercentEngagementProfileAdjuster(-11.0),
            },
            new List<string>(),
            1.5,
            13355.8);
    }

    public double Price(EngagementProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
