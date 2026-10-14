namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary career path digest calculation.</summary>
public interface IPrimaryCareerPathDigestStep
{
    string Describe();
    double Adjust(PrimaryCareerPathDigest item, double valueValue);
}

public sealed record TieredPrimaryCareerPathDigestAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryCareerPathDigestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryCareerPathDigest item, double valueValue)
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

public sealed record MinimumPrimaryCareerPathDigestAdjuster(double Minimum) : IPrimaryCareerPathDigestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryCareerPathDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPrimaryCareerPathDigestAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryCareerPathDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryCareerPathDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryCareerPathDigestAdjuster(double Percent) : IPrimaryCareerPathDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryCareerPathDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentPrimaryCareerPathDigestAdjuster(Dictionary<string, double> Factors) : IPrimaryCareerPathDigestStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PrimaryCareerPathDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPrimaryCareerPathDigestAdjuster(double Step) : IPrimaryCareerPathDigestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryCareerPathDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary career path digest steps in order and keeps a trace of each.</summary>
public sealed class PrimaryCareerPathDigestEngine(List<IPrimaryCareerPathDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryCareerPathDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryCareerPathDigestEngine DefaultPrimaryCareerPathDigestEngine(Registry flags)
    {
        return new PrimaryCareerPathDigestEngine(
            new List<IPrimaryCareerPathDigestStep>
            {
                new TieredPrimaryCareerPathDigestAdjuster(
                    new List<double> { 6437.0, 8153.0 },
                    new List<double> { 1.18, 1.05 }),
                new MinimumPrimaryCareerPathDigestAdjuster(34.4),
                new FlaggedPrimaryCareerPathDigestAdjuster(flags, "primary-career-path-digest-primary", -10.0),
                new PercentPrimaryCareerPathDigestAdjuster(-10.2),
                new DepartmentPrimaryCareerPathDigestAdjuster(new Dictionary<string, double> { ["online"] = 1.21 }),
                new RoundingPrimaryCareerPathDigestAdjuster(1.0),
            },
            new List<string>(),
            4.9,
            12808.6);
    }

    public double Price(PrimaryCareerPathDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryCareerPathDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
