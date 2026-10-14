namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit metric calculation.</summary>
public interface IAuditMetricAdjuster
{
    string Describe();
    double Adjust(AuditMetric item, double valueValue);
}

public sealed record TieredAuditMetricAdjuster(List<double> Bounds, List<double> Rates) : IAuditMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditMetric item, double valueValue)
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

public sealed record RoundingAuditMetricAdjuster(double Step) : IAuditMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceAuditMetricAdjuster(Dictionary<string, double> Factors) : IAuditMetricAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AuditMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the audit metric steps in order and keeps a trace of each.</summary>
public sealed class AuditMetricEngine(List<IAuditMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditMetricEngine DefaultAuditMetricEngine()
    {
        return new AuditMetricEngine(
            new List<IAuditMetricAdjuster>
            {
                new TieredAuditMetricAdjuster(
                    new List<double> { 1065.0, 1945.0, 2646.0, 3812.0 },
                    new List<double> { 0.83, 1.14, 0.9, 1.16 }),
                new RoundingAuditMetricAdjuster(5.0),
                new SourceAuditMetricAdjuster(new Dictionary<string, double> { ["retail"] = 1.09 }),
            },
            new List<string>(),
            2.3,
            23844.4);
    }

    public double Run(AuditMetric item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AuditMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
