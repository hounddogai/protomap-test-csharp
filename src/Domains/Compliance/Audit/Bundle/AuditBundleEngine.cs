namespace Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>One step of the audit bundle calculation.</summary>
public interface IAuditBundleFactor
{
    string Describe();
    double Adjust(AuditBundle item, double valueValue);
}

public sealed record PercentAuditBundleAdjuster(double Percent) : IAuditBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionAuditBundleAdjuster(Dictionary<string, double> Factors) : IAuditBundleFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AuditBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAuditBundleAdjuster(List<double> Bounds, List<double> Rates) : IAuditBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditBundle item, double valueValue)
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

/// <summary>Runs the audit bundle steps in order and keeps a trace of each.</summary>
public sealed class AuditBundleEngine(List<IAuditBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditBundleEngine DefaultAuditBundleEngine()
    {
        return new AuditBundleEngine(
            new List<IAuditBundleFactor>
            {
                new PercentAuditBundleAdjuster(-11.3),
                new RegionAuditBundleAdjuster(new Dictionary<string, double> { ["central"] = 0.84, ["west"] = 0.91 }),
                new TieredAuditBundleAdjuster(
                    new List<double> { 265.0, 4706.0, 7000.0, 7784.0 },
                    new List<double> { 1.01, 1.09, 0.84, 0.99 }),
            },
            new List<string>(),
            0.7,
            69966.3);
    }

    public double Compute(AuditBundle item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AuditBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
