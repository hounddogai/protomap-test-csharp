namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict career path digest calculation.</summary>
public interface IStrictCareerPathDigestAdjuster
{
    string Describe();
    double Adjust(StrictCareerPathDigest item, double valueValue);
}

public sealed record RoundingStrictCareerPathDigestAdjuster(double Step) : IStrictCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictCareerPathDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedStrictCareerPathDigestAdjuster(Registry Flags, string Flag, double Percent) : IStrictCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictCareerPathDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictCareerPathDigestAdjuster(List<double> Bounds, List<double> Rates) : IStrictCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictCareerPathDigest item, double valueValue)
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

public sealed record StrictCareerPathDigestBudgetOffsetAdjuster(double Weight) : IStrictCareerPathDigestAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(StrictCareerPathDigest item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the strict career path digest steps in order and keeps a trace of each.</summary>
public sealed class StrictCareerPathDigestEngine(List<IStrictCareerPathDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictCareerPathDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictCareerPathDigestEngine DefaultStrictCareerPathDigestEngine(Registry flags)
    {
        return new StrictCareerPathDigestEngine(
            new List<IStrictCareerPathDigestAdjuster>
            {
                new RoundingStrictCareerPathDigestAdjuster(5.0),
                new FlaggedStrictCareerPathDigestAdjuster(flags, "strict-career-path-digest-weighted", 5.4),
                new TieredStrictCareerPathDigestAdjuster(
                    new List<double> { 2367.0, 8086.0, 8563.0 },
                    new List<double> { 1.06, 1.09, 1.08 }),
                new StrictCareerPathDigestBudgetOffsetAdjuster(1.74),
            },
            new List<string>(),
            2.0,
            14229.8);
    }

    public double Run(StrictCareerPathDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictCareerPathDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
