namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled succession plan audit calculation.</summary>
public interface IScheduledSuccessionPlanAuditFactor
{
    string Describe();
    double Adjust(ScheduledSuccessionPlanAudit item, double valueValue);
}

public sealed record RoundingScheduledSuccessionPlanAuditAdjuster(double Step) : IScheduledSuccessionPlanAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledSuccessionPlanAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapScheduledSuccessionPlanAuditAdjuster(double Cap) : IScheduledSuccessionPlanAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledSuccessionPlanAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledSuccessionPlanAuditAdjuster(double Percent) : IScheduledSuccessionPlanAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledSuccessionPlanAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledSuccessionPlanAuditAdjuster(double Minimum) : IScheduledSuccessionPlanAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledSuccessionPlanAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledSuccessionPlanAuditDiscountOffsetAdjuster(double Weight) : IScheduledSuccessionPlanAuditFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(ScheduledSuccessionPlanAudit item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the scheduled succession plan audit steps in order and keeps a trace of each.</summary>
public sealed class ScheduledSuccessionPlanAuditEngine(List<IScheduledSuccessionPlanAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledSuccessionPlanAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledSuccessionPlanAuditEngine DefaultScheduledSuccessionPlanAuditEngine()
    {
        return new ScheduledSuccessionPlanAuditEngine(
            new List<IScheduledSuccessionPlanAuditFactor>
            {
                new RoundingScheduledSuccessionPlanAuditAdjuster(1.0),
                new CapScheduledSuccessionPlanAuditAdjuster(24866.0),
                new PercentScheduledSuccessionPlanAuditAdjuster(-22.6),
                new MinimumScheduledSuccessionPlanAuditAdjuster(24.8),
                new ScheduledSuccessionPlanAuditDiscountOffsetAdjuster(1.71),
            },
            new List<string>(),
            1.0,
            62963.8);
    }

    public double Compute(ScheduledSuccessionPlanAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledSuccessionPlanAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
