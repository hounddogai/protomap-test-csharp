namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default return plan quota calculation.</summary>
public interface IDefaultReturnPlanQuotaModifier
{
    string Describe();
    double Adjust(DefaultReturnPlanQuota item, double valueValue);
}

public sealed record CapDefaultReturnPlanQuotaAdjuster(double Cap) : IDefaultReturnPlanQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultReturnPlanQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DefaultReturnPlanQuotaThresholdOffsetAdjuster(double Weight) : IDefaultReturnPlanQuotaModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(DefaultReturnPlanQuota item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedDefaultReturnPlanQuotaAdjuster(Registry Flags, string Flag, double Percent) : IDefaultReturnPlanQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultReturnPlanQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultReturnPlanQuotaAdjuster(double Step) : IDefaultReturnPlanQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultReturnPlanQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameDefaultReturnPlanQuotaAdjuster(Dictionary<string, double> Factors) : IDefaultReturnPlanQuotaModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DefaultReturnPlanQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultReturnPlanQuotaAdjuster(double Minimum) : IDefaultReturnPlanQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultReturnPlanQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default return plan quota steps in order and keeps a trace of each.</summary>
public sealed class DefaultReturnPlanQuotaEngine(List<IDefaultReturnPlanQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReturnPlanQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReturnPlanQuotaEngine DefaultDefaultReturnPlanQuotaEngine(Registry flags)
    {
        return new DefaultReturnPlanQuotaEngine(
            new List<IDefaultReturnPlanQuotaModifier>
            {
                new CapDefaultReturnPlanQuotaAdjuster(44051.0),
                new DefaultReturnPlanQuotaThresholdOffsetAdjuster(1.28),
                new FlaggedDefaultReturnPlanQuotaAdjuster(flags, "default-return-plan-quota-regional", 6.7),
                new RoundingDefaultReturnPlanQuotaAdjuster(0.25),
                new NameDefaultReturnPlanQuotaAdjuster(new Dictionary<string, double> { ["central"] = 1.05 }),
                new MinimumDefaultReturnPlanQuotaAdjuster(41.0),
            },
            new List<string>(),
            0.1,
            36705.9);
    }

    public double Run(DefaultReturnPlanQuota item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReturnPlanQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
