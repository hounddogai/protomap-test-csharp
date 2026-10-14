namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan audit calculation.</summary>
public interface IActionPlanAuditFactor
{
    string Describe();
    double Adjust(ActionPlanAudit item, double valueValue);
}

public sealed record TieredActionPlanAuditAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanAudit item, double valueValue)
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

public sealed record PercentActionPlanAuditAdjuster(double Percent) : IActionPlanAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryActionPlanAuditAdjuster(Dictionary<string, double> Factors) : IActionPlanAuditFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ActionPlanAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumActionPlanAuditAdjuster(double Minimum) : IActionPlanAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedActionPlanAuditAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingActionPlanAuditAdjuster(double Step) : IActionPlanAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the action plan audit steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanAuditEngine(List<IActionPlanAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanAuditEngine DefaultActionPlanAuditEngine(Registry flags)
    {
        return new ActionPlanAuditEngine(
            new List<IActionPlanAuditFactor>
            {
                new TieredActionPlanAuditAdjuster(
                    new List<double> { 5104.0, 8118.0 },
                    new List<double> { 0.96, 0.85 }),
                new PercentActionPlanAuditAdjuster(-1.1),
                new CountryActionPlanAuditAdjuster(new Dictionary<string, double> { ["retail"] = 0.87 }),
                new MinimumActionPlanAuditAdjuster(5.9),
                new FlaggedActionPlanAuditAdjuster(flags, "action-plan-audit-capped", 0.3),
                new RoundingActionPlanAuditAdjuster(0.25),
            },
            new List<string>(),
            3.6,
            9508.6);
    }

    public double Run(ActionPlanAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
