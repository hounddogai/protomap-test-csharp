namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pool quota calculation.</summary>
public interface IPoolQuotaStep
{
    string Describe();
    double Adjust(PoolQuota item, double valueValue);
}

public sealed record FlaggedPoolQuotaAdjuster(Registry Flags, string Flag, double Percent) : IPoolQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPoolQuotaAdjuster(double Percent) : IPoolQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PoolQuotaBudgetOffsetAdjuster(double Weight) : IPoolQuotaStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PoolQuota item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

/// <summary>Runs the pool quota steps in order and keeps a trace of each.</summary>
public sealed class PoolQuotaEngine(List<IPoolQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolQuotaEngine DefaultPoolQuotaEngine(Registry flags)
    {
        return new PoolQuotaEngine(
            new List<IPoolQuotaStep>
            {
                new FlaggedPoolQuotaAdjuster(flags, "pool-quota-default", 3.9),
                new PercentPoolQuotaAdjuster(-8.2),
                new PoolQuotaBudgetOffsetAdjuster(0.68),
            },
            new List<string>(),
            4.0,
            15775.2);
    }

    public double Compute(PoolQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
