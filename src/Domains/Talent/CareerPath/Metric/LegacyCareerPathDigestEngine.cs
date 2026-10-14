namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy career path digest calculation.</summary>
public interface ILegacyCareerPathDigestAdjuster
{
    string Describe();
    double Adjust(LegacyCareerPathDigest item, double valueValue);
}

public sealed record LegacyCareerPathDigestMaximumOffsetAdjuster(double Weight) : ILegacyCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(LegacyCareerPathDigest item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TitleLegacyCareerPathDigestAdjuster(Dictionary<string, double> Factors) : ILegacyCareerPathDigestAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyCareerPathDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyCareerPathDigestAdjuster(double Percent) : ILegacyCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyCareerPathDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyCareerPathDigestAdjuster(double Minimum) : ILegacyCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyCareerPathDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyCareerPathDigestAdjuster(Registry Flags, string Flag, double Percent) : ILegacyCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyCareerPathDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyCareerPathDigestAdjuster(List<double> Bounds, List<double> Rates) : ILegacyCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyCareerPathDigest item, double valueValue)
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

/// <summary>Runs the legacy career path digest steps in order and keeps a trace of each.</summary>
public sealed class LegacyCareerPathDigestEngine(List<ILegacyCareerPathDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyCareerPathDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyCareerPathDigestEngine DefaultLegacyCareerPathDigestEngine(Registry flags)
    {
        return new LegacyCareerPathDigestEngine(
            new List<ILegacyCareerPathDigestAdjuster>
            {
                new LegacyCareerPathDigestMaximumOffsetAdjuster(1.87),
                new TitleLegacyCareerPathDigestAdjuster(new Dictionary<string, double> { ["north"] = 0.75 }),
                new PercentLegacyCareerPathDigestAdjuster(23.6),
                new MinimumLegacyCareerPathDigestAdjuster(44.9),
                new FlaggedLegacyCareerPathDigestAdjuster(flags, "legacy-career-path-digest-scheduled", 5.7),
                new TieredLegacyCareerPathDigestAdjuster(
                    new List<double> { 3030.0, 6389.0, 8978.0 },
                    new List<double> { 1.14, 1.05, 0.9 }),
            },
            new List<string>(),
            2.3,
            92975.9);
    }

    public double Run(LegacyCareerPathDigest item)
    {
        var current = item.ProjectedTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LegacyCareerPathDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
