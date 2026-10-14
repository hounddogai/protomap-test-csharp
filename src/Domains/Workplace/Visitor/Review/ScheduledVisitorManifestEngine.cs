namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled visitor manifest calculation.</summary>
public interface IScheduledVisitorManifestModifier
{
    string Describe();
    double Adjust(ScheduledVisitorManifest item, double valueValue);
}

public sealed record RoundingScheduledVisitorManifestAdjuster(double Step) : IScheduledVisitorManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledVisitorManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredScheduledVisitorManifestAdjuster(List<double> Bounds, List<double> Rates) : IScheduledVisitorManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledVisitorManifest item, double valueValue)
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

public sealed record FlaggedScheduledVisitorManifestAdjuster(Registry Flags, string Flag, double Percent) : IScheduledVisitorManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledVisitorManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledVisitorManifestAdjuster(double Minimum) : IScheduledVisitorManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledVisitorManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledVisitorManifestAdjuster(double Percent) : IScheduledVisitorManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledVisitorManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledVisitorManifestRatioOffsetAdjuster(double Weight) : IScheduledVisitorManifestModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(ScheduledVisitorManifest item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the scheduled visitor manifest steps in order and keeps a trace of each.</summary>
public sealed class ScheduledVisitorManifestEngine(List<IScheduledVisitorManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledVisitorManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledVisitorManifestEngine DefaultScheduledVisitorManifestEngine(Registry flags)
    {
        return new ScheduledVisitorManifestEngine(
            new List<IScheduledVisitorManifestModifier>
            {
                new RoundingScheduledVisitorManifestAdjuster(5.0),
                new TieredScheduledVisitorManifestAdjuster(
                    new List<double> { 4251.0, 4374.0, 5978.0 },
                    new List<double> { 1.13, 1.12, 0.85 }),
                new FlaggedScheduledVisitorManifestAdjuster(flags, "scheduled-visitor-manifest-rolling", -0.0),
                new MinimumScheduledVisitorManifestAdjuster(29.8),
                new PercentScheduledVisitorManifestAdjuster(15.0),
                new ScheduledVisitorManifestRatioOffsetAdjuster(1.76),
            },
            new List<string>(),
            3.8,
            55725.9);
    }

    public double Compute(ScheduledVisitorManifest item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledVisitorManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
