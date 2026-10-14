namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement manifest calculation.</summary>
public interface IEngagementManifestModifier
{
    string Describe();
    double Adjust(EngagementManifest item, double valueValue);
}

public sealed record RoundingEngagementManifestAdjuster(double Step) : IEngagementManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedEngagementManifestAdjuster(Registry Flags, string Flag, double Percent) : IEngagementManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementManifestAdjuster(double Minimum) : IEngagementManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the engagement manifest steps in order and keeps a trace of each.</summary>
public sealed class EngagementManifestEngine(List<IEngagementManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementManifestEngine DefaultEngagementManifestEngine(Registry flags)
    {
        return new EngagementManifestEngine(
            new List<IEngagementManifestModifier>
            {
                new RoundingEngagementManifestAdjuster(0.25),
                new FlaggedEngagementManifestAdjuster(flags, "engagement-manifest-default", -2.0),
                new MinimumEngagementManifestAdjuster(12.1),
            },
            new List<string>(),
            4.6,
            4163.8);
    }

    public double Run(EngagementManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<EngagementManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
