namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted bonus pool quota calculation.</summary>
public interface IWeightedBonusPoolQuotaStep
{
    string Describe();
    double Adjust(WeightedBonusPoolQuota item, double valueValue);
}

public sealed record PercentWeightedBonusPoolQuotaAdjuster(double Percent) : IWeightedBonusPoolQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedBonusPoolQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedBonusPoolQuotaAdjuster(double Cap) : IWeightedBonusPoolQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedBonusPoolQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedWeightedBonusPoolQuotaAdjuster(Registry Flags, string Flag, double Percent) : IWeightedBonusPoolQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedBonusPoolQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedBonusPoolQuotaTenureMonthsOffsetAdjuster(double Weight) : IWeightedBonusPoolQuotaStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(WeightedBonusPoolQuota item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record ChannelWeightedBonusPoolQuotaAdjuster(Dictionary<string, double> Factors) : IWeightedBonusPoolQuotaStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(WeightedBonusPoolQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted bonus pool quota steps in order and keeps a trace of each.</summary>
public sealed class WeightedBonusPoolQuotaEngine(List<IWeightedBonusPoolQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBonusPoolQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBonusPoolQuotaEngine DefaultWeightedBonusPoolQuotaEngine(Registry flags)
    {
        return new WeightedBonusPoolQuotaEngine(
            new List<IWeightedBonusPoolQuotaStep>
            {
                new PercentWeightedBonusPoolQuotaAdjuster(-23.1),
                new CapWeightedBonusPoolQuotaAdjuster(17717.0),
                new FlaggedWeightedBonusPoolQuotaAdjuster(flags, "weighted-bonus-pool-quota-weighted", 1.5),
                new WeightedBonusPoolQuotaTenureMonthsOffsetAdjuster(1.55),
                new ChannelWeightedBonusPoolQuotaAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.98, ["central"] = 0.7 }),
            },
            new List<string>(),
            1.4,
            67318.5);
    }

    public double Run(WeightedBonusPoolQuota item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBonusPoolQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
