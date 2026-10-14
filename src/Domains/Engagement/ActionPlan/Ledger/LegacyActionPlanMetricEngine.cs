namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy action plan metric calculation.</summary>
public interface ILegacyActionPlanMetricAdjuster
{
    string Describe();
    double Adjust(LegacyActionPlanMetric item, double valueValue);
}

public sealed record MinimumLegacyActionPlanMetricAdjuster(double Minimum) : ILegacyActionPlanMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyActionPlanMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLegacyActionPlanMetricAdjuster(List<double> Bounds, List<double> Rates) : ILegacyActionPlanMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyActionPlanMetric item, double valueValue)
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

public sealed record LabelLegacyActionPlanMetricAdjuster(Dictionary<string, double> Factors) : ILegacyActionPlanMetricAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyActionPlanMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyActionPlanMetricAdjuster(Registry Flags, string Flag, double Percent) : ILegacyActionPlanMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyActionPlanMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLegacyActionPlanMetricAdjuster(double Percent) : ILegacyActionPlanMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyActionPlanMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyActionPlanMetricPriorityOffsetAdjuster(double Weight) : ILegacyActionPlanMetricAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(LegacyActionPlanMetric item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the legacy action plan metric steps in order and keeps a trace of each.</summary>
public sealed class LegacyActionPlanMetricEngine(List<ILegacyActionPlanMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyActionPlanMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyActionPlanMetricEngine DefaultLegacyActionPlanMetricEngine(Registry flags)
    {
        return new LegacyActionPlanMetricEngine(
            new List<ILegacyActionPlanMetricAdjuster>
            {
                new MinimumLegacyActionPlanMetricAdjuster(23.5),
                new TieredLegacyActionPlanMetricAdjuster(
                    new List<double> { 4738.0, 7770.0 },
                    new List<double> { 1.19, 1.17 }),
                new LabelLegacyActionPlanMetricAdjuster(new Dictionary<string, double> { ["online"] = 1.29 }),
                new FlaggedLegacyActionPlanMetricAdjuster(flags, "legacy-action-plan-metric-lenient", -6.5),
                new PercentLegacyActionPlanMetricAdjuster(17.4),
                new LegacyActionPlanMetricPriorityOffsetAdjuster(1.92),
            },
            new List<string>(),
            1.5,
            56628.3);
    }

    public double Price(LegacyActionPlanMetric item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyActionPlanMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
