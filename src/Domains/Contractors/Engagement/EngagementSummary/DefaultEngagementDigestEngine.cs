namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default engagement digest calculation.</summary>
public interface IDefaultEngagementDigestFactor
{
    string Describe();
    double Adjust(DefaultEngagementDigest item, double valueValue);
}

public sealed record DefaultEngagementDigestBatchSizeOffsetAdjuster(double Weight) : IDefaultEngagementDigestFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(DefaultEngagementDigest item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapDefaultEngagementDigestAdjuster(double Cap) : IDefaultEngagementDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultEngagementDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultEngagementDigestAdjuster(Registry Flags, string Flag, double Percent) : IDefaultEngagementDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultEngagementDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default engagement digest steps in order and keeps a trace of each.</summary>
public sealed class DefaultEngagementDigestEngine(List<IDefaultEngagementDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultEngagementDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultEngagementDigestEngine DefaultDefaultEngagementDigestEngine(Registry flags)
    {
        return new DefaultEngagementDigestEngine(
            new List<IDefaultEngagementDigestFactor>
            {
                new DefaultEngagementDigestBatchSizeOffsetAdjuster(0.05),
                new CapDefaultEngagementDigestAdjuster(23068.0),
                new FlaggedDefaultEngagementDigestAdjuster(flags, "default-engagement-digest-seasonal", -6.7),
            },
            new List<string>(),
            0.3,
            48251.4);
    }

    public double Evaluate(DefaultEngagementDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultEngagementDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
