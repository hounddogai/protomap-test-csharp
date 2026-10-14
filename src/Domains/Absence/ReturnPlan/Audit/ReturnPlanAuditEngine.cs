namespace Acme.Workforce.Absence.ReturnPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan audit calculation.</summary>
public interface IReturnPlanAuditAdjuster
{
    string Describe();
    double Adjust(ReturnPlanAudit item, double valueValue);
}

public sealed record TieredReturnPlanAuditAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanAudit item, double valueValue)
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

public sealed record NameReturnPlanAuditAdjuster(Dictionary<string, double> Factors) : IReturnPlanAuditAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ReturnPlanAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapReturnPlanAuditAdjuster(double Cap) : IReturnPlanAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingReturnPlanAuditAdjuster(double Step) : IReturnPlanAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumReturnPlanAuditAdjuster(double Minimum) : IReturnPlanAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedReturnPlanAuditAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan audit steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanAuditEngine(List<IReturnPlanAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanAuditEngine DefaultReturnPlanAuditEngine(Registry flags)
    {
        return new ReturnPlanAuditEngine(
            new List<IReturnPlanAuditAdjuster>
            {
                new TieredReturnPlanAuditAdjuster(
                    new List<double> { 188.0, 3212.0, 3336.0, 8335.0 },
                    new List<double> { 0.84, 0.89, 1.02, 1.05 }),
                new NameReturnPlanAuditAdjuster(new Dictionary<string, double> { ["east"] = 1.25 }),
                new CapReturnPlanAuditAdjuster(11995.0),
                new RoundingReturnPlanAuditAdjuster(0.05),
                new MinimumReturnPlanAuditAdjuster(29.7),
                new FlaggedReturnPlanAuditAdjuster(flags, "return-plan-audit-tiered", -8.6),
            },
            new List<string>(),
            2.7,
            1731.3);
    }

    public double Price(ReturnPlanAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
