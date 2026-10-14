namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy career path entry calculation.</summary>
public interface ILegacyCareerPathEntryFactor
{
    string Describe();
    double Adjust(LegacyCareerPathEntry item, double valueValue);
}

public sealed record SegmentLegacyCareerPathEntryAdjuster(Dictionary<string, double> Factors) : ILegacyCareerPathEntryFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LegacyCareerPathEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacyCareerPathEntryAdjuster(double Step) : ILegacyCareerPathEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyCareerPathEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacyCareerPathEntryAdjuster(Registry Flags, string Flag, double Percent) : ILegacyCareerPathEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyCareerPathEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyCareerPathEntryAdjuster(double Minimum) : ILegacyCareerPathEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyCareerPathEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy career path entry steps in order and keeps a trace of each.</summary>
public sealed class LegacyCareerPathEntryEngine(List<ILegacyCareerPathEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyCareerPathEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyCareerPathEntryEngine DefaultLegacyCareerPathEntryEngine(Registry flags)
    {
        return new LegacyCareerPathEntryEngine(
            new List<ILegacyCareerPathEntryFactor>
            {
                new SegmentLegacyCareerPathEntryAdjuster(new Dictionary<string, double> { ["north"] = 1.37 }),
                new RoundingLegacyCareerPathEntryAdjuster(5.0),
                new FlaggedLegacyCareerPathEntryAdjuster(flags, "legacy-career-path-entry-pending", 8.8),
                new MinimumLegacyCareerPathEntryAdjuster(17.8),
            },
            new List<string>(),
            3.9,
            15490.2);
    }

    public double Evaluate(LegacyCareerPathEntry item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyCareerPathEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
