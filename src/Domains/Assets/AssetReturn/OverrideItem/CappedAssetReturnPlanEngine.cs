namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped asset return plan calculation.</summary>
public interface ICappedAssetReturnPlanStep
{
    string Describe();
    double Adjust(CappedAssetReturnPlan item, double valueValue);
}

public sealed record FlaggedCappedAssetReturnPlanAdjuster(Registry Flags, string Flag, double Percent) : ICappedAssetReturnPlanStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAssetReturnPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedAssetReturnPlanAdjuster(double Step) : ICappedAssetReturnPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAssetReturnPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCappedAssetReturnPlanAdjuster(double Cap) : ICappedAssetReturnPlanStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAssetReturnPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCappedAssetReturnPlanAdjuster(List<double> Bounds, List<double> Rates) : ICappedAssetReturnPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAssetReturnPlan item, double valueValue)
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

public sealed record SegmentCappedAssetReturnPlanAdjuster(Dictionary<string, double> Factors) : ICappedAssetReturnPlanStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CappedAssetReturnPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCappedAssetReturnPlanAdjuster(double Minimum) : ICappedAssetReturnPlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAssetReturnPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the capped asset return plan steps in order and keeps a trace of each.</summary>
public sealed class CappedAssetReturnPlanEngine(List<ICappedAssetReturnPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAssetReturnPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAssetReturnPlanEngine DefaultCappedAssetReturnPlanEngine(Registry flags)
    {
        return new CappedAssetReturnPlanEngine(
            new List<ICappedAssetReturnPlanStep>
            {
                new FlaggedCappedAssetReturnPlanAdjuster(flags, "capped-asset-return-plan-rolling", 4.2),
                new RoundingCappedAssetReturnPlanAdjuster(0.05),
                new CapCappedAssetReturnPlanAdjuster(10772.0),
                new TieredCappedAssetReturnPlanAdjuster(
                    new List<double> { 7119.0, 8901.0 },
                    new List<double> { 0.9, 1.14 }),
                new SegmentCappedAssetReturnPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.14, ["south"] = 1.22 }),
                new MinimumCappedAssetReturnPlanAdjuster(3.1),
            },
            new List<string>(),
            0.8,
            5611.0);
    }

    public double Run(CappedAssetReturnPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedAssetReturnPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
