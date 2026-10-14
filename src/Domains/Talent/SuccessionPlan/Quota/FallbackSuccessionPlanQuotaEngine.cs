namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback succession plan quota calculation.</summary>
public interface IFallbackSuccessionPlanQuotaAdjuster
{
    string Describe();
    double Adjust(FallbackSuccessionPlanQuota item, double valueValue);
}

public sealed record FlaggedFallbackSuccessionPlanQuotaAdjuster(Registry Flags, string Flag, double Percent) : IFallbackSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackSuccessionPlanQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackSuccessionPlanQuotaEndsAtOffsetAdjuster(double Weight) : IFallbackSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(FallbackSuccessionPlanQuota item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record PercentFallbackSuccessionPlanQuotaAdjuster(double Percent) : IFallbackSuccessionPlanQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackSuccessionPlanQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback succession plan quota steps in order and keeps a trace of each.</summary>
public sealed class FallbackSuccessionPlanQuotaEngine(List<IFallbackSuccessionPlanQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSuccessionPlanQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSuccessionPlanQuotaEngine DefaultFallbackSuccessionPlanQuotaEngine(Registry flags)
    {
        return new FallbackSuccessionPlanQuotaEngine(
            new List<IFallbackSuccessionPlanQuotaAdjuster>
            {
                new FlaggedFallbackSuccessionPlanQuotaAdjuster(flags, "fallback-succession-plan-quota-tiered", 4.7),
                new FallbackSuccessionPlanQuotaEndsAtOffsetAdjuster(0.38),
                new PercentFallbackSuccessionPlanQuotaAdjuster(-7.9),
            },
            new List<string>(),
            0.9,
            70291.1);
    }

    public double Evaluate(FallbackSuccessionPlanQuota item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSuccessionPlanQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
