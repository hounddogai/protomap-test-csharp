namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient floor plan audit calculation.</summary>
public interface ILenientFloorPlanAuditFactor
{
    string Describe();
    double Adjust(LenientFloorPlanAudit item, double valueValue);
}

public sealed record PercentLenientFloorPlanAuditAdjuster(double Percent) : ILenientFloorPlanAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientFloorPlanAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientFloorPlanAuditAdjuster(double Step) : ILenientFloorPlanAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientFloorPlanAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientFloorPlanAuditAdjuster(Registry Flags, string Flag, double Percent) : ILenientFloorPlanAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientFloorPlanAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientFloorPlanAuditAdjuster(double Minimum) : ILenientFloorPlanAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientFloorPlanAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameLenientFloorPlanAuditAdjuster(Dictionary<string, double> Factors) : ILenientFloorPlanAuditFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientFloorPlanAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient floor plan audit steps in order and keeps a trace of each.</summary>
public sealed class LenientFloorPlanAuditEngine(List<ILenientFloorPlanAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientFloorPlanAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientFloorPlanAuditEngine DefaultLenientFloorPlanAuditEngine(Registry flags)
    {
        return new LenientFloorPlanAuditEngine(
            new List<ILenientFloorPlanAuditFactor>
            {
                new PercentLenientFloorPlanAuditAdjuster(-17.9),
                new RoundingLenientFloorPlanAuditAdjuster(0.05),
                new FlaggedLenientFloorPlanAuditAdjuster(flags, "lenient-floor-plan-audit-default", -4.1),
                new MinimumLenientFloorPlanAuditAdjuster(27.9),
                new NameLenientFloorPlanAuditAdjuster(new Dictionary<string, double> { ["retail"] = 1.17 }),
            },
            new List<string>(),
            4.5,
            21889.1);
    }

    public double Price(LenientFloorPlanAudit item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientFloorPlanAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
