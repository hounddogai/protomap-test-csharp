namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default return plan audit calculation.</summary>
public interface IDefaultReturnPlanAuditModifier
{
    string Describe();
    double Adjust(DefaultReturnPlanAudit item, double valueValue);
}

public sealed record TieredDefaultReturnPlanAuditAdjuster(List<double> Bounds, List<double> Rates) : IDefaultReturnPlanAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultReturnPlanAudit item, double valueValue)
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

public sealed record CategoryDefaultReturnPlanAuditAdjuster(Dictionary<string, double> Factors) : IDefaultReturnPlanAuditModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(DefaultReturnPlanAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDefaultReturnPlanAuditAdjuster(double Cap) : IDefaultReturnPlanAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultReturnPlanAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultReturnPlanAuditAdjuster(Registry Flags, string Flag, double Percent) : IDefaultReturnPlanAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultReturnPlanAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultReturnPlanAuditAdjuster(double Percent) : IDefaultReturnPlanAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultReturnPlanAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default return plan audit steps in order and keeps a trace of each.</summary>
public sealed class DefaultReturnPlanAuditEngine(List<IDefaultReturnPlanAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReturnPlanAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReturnPlanAuditEngine DefaultDefaultReturnPlanAuditEngine(Registry flags)
    {
        return new DefaultReturnPlanAuditEngine(
            new List<IDefaultReturnPlanAuditModifier>
            {
                new TieredDefaultReturnPlanAuditAdjuster(
                    new List<double> { 483.0, 3567.0 },
                    new List<double> { 1.13, 0.89 }),
                new CategoryDefaultReturnPlanAuditAdjuster(new Dictionary<string, double> { ["online"] = 0.76 }),
                new CapDefaultReturnPlanAuditAdjuster(10360.0),
                new FlaggedDefaultReturnPlanAuditAdjuster(flags, "default-return-plan-audit-adaptive", -1.6),
                new PercentDefaultReturnPlanAuditAdjuster(-4.3),
            },
            new List<string>(),
            0.8,
            21374.2);
    }

    public double Compute(DefaultReturnPlanAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReturnPlanAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
