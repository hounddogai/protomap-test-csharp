namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default access badge manifest calculation.</summary>
public interface IDefaultAccessBadgeManifestFactor
{
    string Describe();
    double Adjust(DefaultAccessBadgeManifest item, double valueValue);
}

public sealed record SegmentDefaultAccessBadgeManifestAdjuster(Dictionary<string, double> Factors) : IDefaultAccessBadgeManifestFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DefaultAccessBadgeManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultAccessBadgeManifestAdjuster(double Cap) : IDefaultAccessBadgeManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAccessBadgeManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDefaultAccessBadgeManifestAdjuster(double Step) : IDefaultAccessBadgeManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultAccessBadgeManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultAccessBadgeManifestAdjuster(double Percent) : IDefaultAccessBadgeManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAccessBadgeManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultAccessBadgeManifestAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAccessBadgeManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAccessBadgeManifest item, double valueValue)
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

public sealed record MinimumDefaultAccessBadgeManifestAdjuster(double Minimum) : IDefaultAccessBadgeManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAccessBadgeManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default access badge manifest steps in order and keeps a trace of each.</summary>
public sealed class DefaultAccessBadgeManifestEngine(List<IDefaultAccessBadgeManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAccessBadgeManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAccessBadgeManifestEngine DefaultDefaultAccessBadgeManifestEngine()
    {
        return new DefaultAccessBadgeManifestEngine(
            new List<IDefaultAccessBadgeManifestFactor>
            {
                new SegmentDefaultAccessBadgeManifestAdjuster(new Dictionary<string, double> { ["north"] = 0.71 }),
                new CapDefaultAccessBadgeManifestAdjuster(33411.0),
                new RoundingDefaultAccessBadgeManifestAdjuster(5.0),
                new PercentDefaultAccessBadgeManifestAdjuster(-24.2),
                new TieredDefaultAccessBadgeManifestAdjuster(
                    new List<double> { 4342.0, 6545.0 },
                    new List<double> { 1.02, 0.88 }),
                new MinimumDefaultAccessBadgeManifestAdjuster(25.8),
            },
            new List<string>(),
            2.4,
            45008.9);
    }

    public double Evaluate(DefaultAccessBadgeManifest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAccessBadgeManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
