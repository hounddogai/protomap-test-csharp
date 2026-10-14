namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default return plan tier calculation.</summary>
public interface IDefaultReturnPlanTierStep
{
    string Describe();
    double Adjust(DefaultReturnPlanTier item, double valueValue);
}

public sealed record DefaultReturnPlanTierCreatedAtOffsetAdjuster(double Weight) : IDefaultReturnPlanTierStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(DefaultReturnPlanTier item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingDefaultReturnPlanTierAdjuster(double Step) : IDefaultReturnPlanTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultReturnPlanTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultReturnPlanTierAdjuster(Registry Flags, string Flag, double Percent) : IDefaultReturnPlanTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultReturnPlanTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelDefaultReturnPlanTierAdjuster(Dictionary<string, double> Factors) : IDefaultReturnPlanTierStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(DefaultReturnPlanTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDefaultReturnPlanTierAdjuster(double Percent) : IDefaultReturnPlanTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultReturnPlanTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultReturnPlanTierAdjuster(List<double> Bounds, List<double> Rates) : IDefaultReturnPlanTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultReturnPlanTier item, double valueValue)
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

/// <summary>Runs the default return plan tier steps in order and keeps a trace of each.</summary>
public sealed class DefaultReturnPlanTierEngine(List<IDefaultReturnPlanTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReturnPlanTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReturnPlanTierEngine DefaultDefaultReturnPlanTierEngine(Registry flags)
    {
        return new DefaultReturnPlanTierEngine(
            new List<IDefaultReturnPlanTierStep>
            {
                new DefaultReturnPlanTierCreatedAtOffsetAdjuster(0.17),
                new RoundingDefaultReturnPlanTierAdjuster(5.0),
                new FlaggedDefaultReturnPlanTierAdjuster(flags, "default-return-plan-tier-pending", -9.3),
                new ChannelDefaultReturnPlanTierAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.01, ["north"] = 1.39, ["east"] = 1.3 }),
                new PercentDefaultReturnPlanTierAdjuster(-20.3),
                new TieredDefaultReturnPlanTierAdjuster(
                    new List<double> { 2913.0, 9302.0 },
                    new List<double> { 1.18, 0.88 }),
            },
            new List<string>(),
            4.9,
            41654.4);
    }

    public double Run(DefaultReturnPlanTier item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReturnPlanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
