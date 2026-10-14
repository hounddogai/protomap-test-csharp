namespace Acme.Workforce.Talent.Pool.Audit;

/// <summary>One step of the pool audit calculation.</summary>
public interface IPoolAuditFactor
{
    string Describe();
    double Adjust(PoolAudit item, double valueValue);
}

public sealed record TieredPoolAuditAdjuster(List<double> Bounds, List<double> Rates) : IPoolAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolAudit item, double valueValue)
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

public sealed record SegmentPoolAuditAdjuster(Dictionary<string, double> Factors) : IPoolAuditFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PoolAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPoolAuditAdjuster(double Cap) : IPoolAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pool audit steps in order and keeps a trace of each.</summary>
public sealed class PoolAuditEngine(List<IPoolAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolAuditEngine DefaultPoolAuditEngine()
    {
        return new PoolAuditEngine(
            new List<IPoolAuditFactor>
            {
                new TieredPoolAuditAdjuster(
                    new List<double> { 3451.0, 6393.0, 6681.0 },
                    new List<double> { 0.99, 0.98, 1.02 }),
                new SegmentPoolAuditAdjuster(new Dictionary<string, double> { ["central"] = 1.36, ["east"] = 1.25 }),
                new CapPoolAuditAdjuster(28592.0),
            },
            new List<string>(),
            0.1,
            70680.9);
    }

    public double Compute(PoolAudit item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PoolAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
