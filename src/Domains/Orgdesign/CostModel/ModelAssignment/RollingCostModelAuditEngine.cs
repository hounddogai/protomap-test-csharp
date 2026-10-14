namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling cost model audit calculation.</summary>
public interface IRollingCostModelAuditAdjuster
{
    string Describe();
    double Adjust(RollingCostModelAudit item, double valueValue);
}

public sealed record RoundingRollingCostModelAuditAdjuster(double Step) : IRollingCostModelAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingCostModelAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingCostModelAuditAdjuster(double Minimum) : IRollingCostModelAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingCostModelAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRollingCostModelAuditAdjuster(Dictionary<string, double> Factors) : IRollingCostModelAuditAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RollingCostModelAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRollingCostModelAuditAdjuster(List<double> Bounds, List<double> Rates) : IRollingCostModelAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingCostModelAudit item, double valueValue)
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

public sealed record FlaggedRollingCostModelAuditAdjuster(Registry Flags, string Flag, double Percent) : IRollingCostModelAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingCostModelAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRollingCostModelAuditAdjuster(double Percent) : IRollingCostModelAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingCostModelAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling cost model audit steps in order and keeps a trace of each.</summary>
public sealed class RollingCostModelAuditEngine(List<IRollingCostModelAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingCostModelAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingCostModelAuditEngine DefaultRollingCostModelAuditEngine(Registry flags)
    {
        return new RollingCostModelAuditEngine(
            new List<IRollingCostModelAuditAdjuster>
            {
                new RoundingRollingCostModelAuditAdjuster(1.0),
                new MinimumRollingCostModelAuditAdjuster(12.7),
                new NameRollingCostModelAuditAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.02, ["retail"] = 0.91, ["south"] = 1.02 }),
                new TieredRollingCostModelAuditAdjuster(
                    new List<double> { 4598.0, 6421.0, 9930.0 },
                    new List<double> { 1.15, 1.03, 0.81 }),
                new FlaggedRollingCostModelAuditAdjuster(flags, "rolling-cost-model-audit-lenient", 7.2),
                new PercentRollingCostModelAuditAdjuster(9.8),
            },
            new List<string>(),
            4.5,
            1952.9);
    }

    public double Evaluate(RollingCostModelAudit item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingCostModelAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
