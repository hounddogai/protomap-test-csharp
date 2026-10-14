namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy occupancy digest calculation.</summary>
public interface ILegacyOccupancyDigestStep
{
    string Describe();
    double Adjust(LegacyOccupancyDigest item, double valueValue);
}

public sealed record NameLegacyOccupancyDigestAdjuster(Dictionary<string, double> Factors) : ILegacyOccupancyDigestStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacyOccupancyDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyOccupancyDigestAdjuster(Registry Flags, string Flag, double Percent) : ILegacyOccupancyDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyOccupancyDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyOccupancyDigestAdjuster(double Step) : ILegacyOccupancyDigestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyOccupancyDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyOccupancyDigestAdjuster(double Percent) : ILegacyOccupancyDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyOccupancyDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyOccupancyDigestAdjuster(List<double> Bounds, List<double> Rates) : ILegacyOccupancyDigestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyOccupancyDigest item, double valueValue)
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

/// <summary>Runs the legacy occupancy digest steps in order and keeps a trace of each.</summary>
public sealed class LegacyOccupancyDigestEngine(List<ILegacyOccupancyDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyOccupancyDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyOccupancyDigestEngine DefaultLegacyOccupancyDigestEngine(Registry flags)
    {
        return new LegacyOccupancyDigestEngine(
            new List<ILegacyOccupancyDigestStep>
            {
                new NameLegacyOccupancyDigestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.2, ["east"] = 1.18, ["central"] = 1.04 }),
                new FlaggedLegacyOccupancyDigestAdjuster(flags, "legacy-occupancy-digest-regional", 5.9),
                new RoundingLegacyOccupancyDigestAdjuster(1.0),
                new PercentLegacyOccupancyDigestAdjuster(13.3),
                new TieredLegacyOccupancyDigestAdjuster(
                    new List<double> { 2251.0, 3652.0 },
                    new List<double> { 1.12, 1.14 }),
            },
            new List<string>(),
            1.1,
            93989.9);
    }

    public double Price(LegacyOccupancyDigest item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyOccupancyDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
