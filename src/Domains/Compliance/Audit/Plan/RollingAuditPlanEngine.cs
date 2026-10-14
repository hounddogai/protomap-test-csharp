namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling audit plan calculation.</summary>
public interface IRollingAuditPlanFactor
{
    string Describe();
    double Adjust(RollingAuditPlan item, double valueValue);
}

public sealed record PercentRollingAuditPlanAdjuster(double Percent) : IRollingAuditPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAuditPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingAuditPlanAdjuster(double Minimum) : IRollingAuditPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAuditPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleRollingAuditPlanAdjuster(Dictionary<string, double> Factors) : IRollingAuditPlanFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingAuditPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling audit plan steps in order and keeps a trace of each.</summary>
public sealed class RollingAuditPlanEngine(List<IRollingAuditPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAuditPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAuditPlanEngine DefaultRollingAuditPlanEngine()
    {
        return new RollingAuditPlanEngine(
            new List<IRollingAuditPlanFactor>
            {
                new PercentRollingAuditPlanAdjuster(-21.8),
                new MinimumRollingAuditPlanAdjuster(29.6),
                new TitleRollingAuditPlanAdjuster(new Dictionary<string, double> { ["retail"] = 1.18 }),
            },
            new List<string>(),
            0.3,
            79304.8);
    }

    public double Compute(RollingAuditPlan item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAuditPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
