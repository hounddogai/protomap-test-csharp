namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy asset return plan calculation.</summary>
public interface ILegacyAssetReturnPlanModifier
{
    string Describe();
    double Adjust(LegacyAssetReturnPlan item, double valueValue);
}

public sealed record FlaggedLegacyAssetReturnPlanAdjuster(Registry Flags, string Flag, double Percent) : ILegacyAssetReturnPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyAssetReturnPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLegacyAssetReturnPlanAdjuster(Dictionary<string, double> Factors) : ILegacyAssetReturnPlanModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyAssetReturnPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacyAssetReturnPlanDiscountOffsetAdjuster(double Weight) : ILegacyAssetReturnPlanModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LegacyAssetReturnPlan item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record MinimumLegacyAssetReturnPlanAdjuster(double Minimum) : ILegacyAssetReturnPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyAssetReturnPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyAssetReturnPlanAdjuster(double Percent) : ILegacyAssetReturnPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyAssetReturnPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy asset return plan steps in order and keeps a trace of each.</summary>
public sealed class LegacyAssetReturnPlanEngine(List<ILegacyAssetReturnPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAssetReturnPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAssetReturnPlanEngine DefaultLegacyAssetReturnPlanEngine(Registry flags)
    {
        return new LegacyAssetReturnPlanEngine(
            new List<ILegacyAssetReturnPlanModifier>
            {
                new FlaggedLegacyAssetReturnPlanAdjuster(flags, "legacy-asset-return-plan-capped", -3.7),
                new TitleLegacyAssetReturnPlanAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.98, ["central"] = 0.8 }),
                new LegacyAssetReturnPlanDiscountOffsetAdjuster(0.31),
                new MinimumLegacyAssetReturnPlanAdjuster(34.1),
                new PercentLegacyAssetReturnPlanAdjuster(-13.1),
            },
            new List<string>(),
            0.6,
            12966.4);
    }

    public double Price(LegacyAssetReturnPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAssetReturnPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
