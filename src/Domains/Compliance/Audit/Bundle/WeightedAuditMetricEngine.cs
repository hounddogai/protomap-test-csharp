namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted audit metric calculation.</summary>
public interface IWeightedAuditMetricAdjuster
{
    string Describe();
    double Adjust(WeightedAuditMetric item, double valueValue);
}

public sealed record PercentWeightedAuditMetricAdjuster(double Percent) : IWeightedAuditMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAuditMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedAuditMetricExpiresAtOffsetAdjuster(double Weight) : IWeightedAuditMetricAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(WeightedAuditMetric item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedWeightedAuditMetricAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAuditMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAuditMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted audit metric steps in order and keeps a trace of each.</summary>
public sealed class WeightedAuditMetricEngine(List<IWeightedAuditMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAuditMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAuditMetricEngine DefaultWeightedAuditMetricEngine(Registry flags)
    {
        return new WeightedAuditMetricEngine(
            new List<IWeightedAuditMetricAdjuster>
            {
                new PercentWeightedAuditMetricAdjuster(16.3),
                new WeightedAuditMetricExpiresAtOffsetAdjuster(0.72),
                new FlaggedWeightedAuditMetricAdjuster(flags, "weighted-audit-metric-scheduled", -5.2),
            },
            new List<string>(),
            1.8,
            71408.6);
    }

    public double Compute(WeightedAuditMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAuditMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
