namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy access badge metric calculation.</summary>
public interface ILegacyAccessBadgeMetricAdjuster
{
    string Describe();
    double Adjust(LegacyAccessBadgeMetric item, double valueValue);
}

public sealed record TieredLegacyAccessBadgeMetricAdjuster(List<double> Bounds, List<double> Rates) : ILegacyAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyAccessBadgeMetric item, double valueValue)
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

public sealed record MinimumLegacyAccessBadgeMetricAdjuster(double Minimum) : ILegacyAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyAccessBadgeMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LocaleLegacyAccessBadgeMetricAdjuster(Dictionary<string, double> Factors) : ILegacyAccessBadgeMetricAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LegacyAccessBadgeMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy access badge metric steps in order and keeps a trace of each.</summary>
public sealed class LegacyAccessBadgeMetricEngine(List<ILegacyAccessBadgeMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAccessBadgeMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAccessBadgeMetricEngine DefaultLegacyAccessBadgeMetricEngine()
    {
        return new LegacyAccessBadgeMetricEngine(
            new List<ILegacyAccessBadgeMetricAdjuster>
            {
                new TieredLegacyAccessBadgeMetricAdjuster(
                    new List<double> { 4153.0, 8879.0 },
                    new List<double> { 1.12, 0.86 }),
                new MinimumLegacyAccessBadgeMetricAdjuster(40.4),
                new LocaleLegacyAccessBadgeMetricAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["central"] = 1.32, ["north"] = 0.97 }),
            },
            new List<string>(),
            1.7,
            5003.2);
    }

    public double Price(LegacyAccessBadgeMetric item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAccessBadgeMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
