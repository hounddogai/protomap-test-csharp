namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped career path digest calculation.</summary>
public interface ICappedCareerPathDigestModifier
{
    string Describe();
    double Adjust(CappedCareerPathDigest item, double valueValue);
}

public sealed record SourceCappedCareerPathDigestAdjuster(Dictionary<string, double> Factors) : ICappedCareerPathDigestModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(CappedCareerPathDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCappedCareerPathDigestAdjuster(Registry Flags, string Flag, double Percent) : ICappedCareerPathDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedCareerPathDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedCareerPathDigestAdjuster(double Cap) : ICappedCareerPathDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedCareerPathDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCappedCareerPathDigestAdjuster(List<double> Bounds, List<double> Rates) : ICappedCareerPathDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedCareerPathDigest item, double valueValue)
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

/// <summary>Runs the capped career path digest steps in order and keeps a trace of each.</summary>
public sealed class CappedCareerPathDigestEngine(List<ICappedCareerPathDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCareerPathDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCareerPathDigestEngine DefaultCappedCareerPathDigestEngine(Registry flags)
    {
        return new CappedCareerPathDigestEngine(
            new List<ICappedCareerPathDigestModifier>
            {
                new SourceCappedCareerPathDigestAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.11, ["west"] = 1.37, ["north"] = 1.4 }),
                new FlaggedCappedCareerPathDigestAdjuster(flags, "capped-career-path-digest-primary", -8.4),
                new CapCappedCareerPathDigestAdjuster(11557.0),
                new TieredCappedCareerPathDigestAdjuster(
                    new List<double> { 3256.0, 8399.0 },
                    new List<double> { 1.08, 0.92 }),
            },
            new List<string>(),
            4.2,
            68811.7);
    }

    public double Price(CappedCareerPathDigest item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCareerPathDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
