namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling succession plan quota calculation.</summary>
public interface IRollingSuccessionPlanQuotaAdjuster
{
    string Describe();
    double Adjust(RollingSuccessionPlanQuota item, double valueValue);
}

public sealed record FlaggedRollingSuccessionPlanQuotaAdjuster(Registry Flags, string Flag, double Percent) : IRollingSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingSuccessionPlanQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingSuccessionPlanQuotaAdjuster(double Cap) : IRollingSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingSuccessionPlanQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceRollingSuccessionPlanQuotaAdjuster(Dictionary<string, double> Factors) : IRollingSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RollingSuccessionPlanQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRollingSuccessionPlanQuotaAdjuster(double Percent) : IRollingSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingSuccessionPlanQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling succession plan quota steps in order and keeps a trace of each.</summary>
public sealed class RollingSuccessionPlanQuotaEngine(List<IRollingSuccessionPlanQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSuccessionPlanQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSuccessionPlanQuotaEngine DefaultRollingSuccessionPlanQuotaEngine(Registry flags)
    {
        return new RollingSuccessionPlanQuotaEngine(
            new List<IRollingSuccessionPlanQuotaAdjuster>
            {
                new FlaggedRollingSuccessionPlanQuotaAdjuster(flags, "rolling-succession-plan-quota-seasonal", 4.1),
                new CapRollingSuccessionPlanQuotaAdjuster(15366.0),
                new SourceRollingSuccessionPlanQuotaAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.86, ["retail"] = 1.2, ["east"] = 1.04 }),
                new PercentRollingSuccessionPlanQuotaAdjuster(2.4),
            },
            new List<string>(),
            0.5,
            74380.4);
    }

    public double Price(RollingSuccessionPlanQuota item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingSuccessionPlanQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
