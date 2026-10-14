namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adjustment plan calculation.</summary>
public interface IAdjustmentPlanModifier
{
    string Describe();
    double Adjust(AdjustmentPlan item, double valueValue);
}

public sealed record FlaggedAdjustmentPlanAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdjustmentPlanAdjuster(double Percent) : IAdjustmentPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdjustmentPlanCapacityOffsetAdjuster(double Weight) : IAdjustmentPlanModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(AdjustmentPlan item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record LabelAdjustmentPlanAdjuster(Dictionary<string, double> Factors) : IAdjustmentPlanModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AdjustmentPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdjustmentPlanAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentPlan item, double valueValue)
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

/// <summary>Runs the adjustment plan steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentPlanEngine(List<IAdjustmentPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentPlanEngine DefaultAdjustmentPlanEngine(Registry flags)
    {
        return new AdjustmentPlanEngine(
            new List<IAdjustmentPlanModifier>
            {
                new FlaggedAdjustmentPlanAdjuster(flags, "adjustment-plan-global", 9.1),
                new PercentAdjustmentPlanAdjuster(-9.7),
                new AdjustmentPlanCapacityOffsetAdjuster(1.82),
                new LabelAdjustmentPlanAdjuster(new Dictionary<string, double> { ["east"] = 1.0, ["online"] = 1.21 }),
                new TieredAdjustmentPlanAdjuster(
                    new List<double> { 2055.0, 3952.0, 9248.0 },
                    new List<double> { 0.88, 1.12, 1.05 }),
            },
            new List<string>(),
            4.3,
            13604.7);
    }

    public double Compute(AdjustmentPlan item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
