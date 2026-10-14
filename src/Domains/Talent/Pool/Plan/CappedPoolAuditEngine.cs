namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped pool audit calculation.</summary>
public interface ICappedPoolAuditStep
{
    string Describe();
    double Adjust(CappedPoolAudit item, double valueValue);
}

public sealed record FlaggedCappedPoolAuditAdjuster(Registry Flags, string Flag, double Percent) : ICappedPoolAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPoolAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedPoolAuditBudgetOffsetAdjuster(double Weight) : ICappedPoolAuditStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(CappedPoolAudit item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapCappedPoolAuditAdjuster(double Cap) : ICappedPoolAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPoolAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCappedPoolAuditAdjuster(List<double> Bounds, List<double> Rates) : ICappedPoolAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPoolAudit item, double valueValue)
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

public sealed record LabelCappedPoolAuditAdjuster(Dictionary<string, double> Factors) : ICappedPoolAuditStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedPoolAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped pool audit steps in order and keeps a trace of each.</summary>
public sealed class CappedPoolAuditEngine(List<ICappedPoolAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPoolAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPoolAuditEngine DefaultCappedPoolAuditEngine(Registry flags)
    {
        return new CappedPoolAuditEngine(
            new List<ICappedPoolAuditStep>
            {
                new FlaggedCappedPoolAuditAdjuster(flags, "capped-pool-audit-legacy", -7.6),
                new CappedPoolAuditBudgetOffsetAdjuster(1.58),
                new CapCappedPoolAuditAdjuster(4605.0),
                new TieredCappedPoolAuditAdjuster(
                    new List<double> { 5710.0, 7657.0, 7705.0 },
                    new List<double> { 0.89, 1.12, 1.19 }),
                new LabelCappedPoolAuditAdjuster(new Dictionary<string, double> { ["central"] = 1.06 }),
            },
            new List<string>(),
            0.6,
            78812.9);
    }

    public double Run(CappedPoolAudit item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPoolAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
