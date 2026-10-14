namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending asset return adjustment calculation.</summary>
public interface IPendingAssetReturnAdjustmentFactor
{
    string Describe();
    double Adjust(PendingAssetReturnAdjustment item, double valueValue);
}

public sealed record RoundingPendingAssetReturnAdjustmentAdjuster(double Step) : IPendingAssetReturnAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingAssetReturnAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PendingAssetReturnAdjustmentBudgetOffsetAdjuster(double Weight) : IPendingAssetReturnAdjustmentFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PendingAssetReturnAdjustment item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record FlaggedPendingAssetReturnAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IPendingAssetReturnAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingAssetReturnAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending asset return adjustment steps in order and keeps a trace of each.</summary>
public sealed class PendingAssetReturnAdjustmentEngine(List<IPendingAssetReturnAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAssetReturnAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAssetReturnAdjustmentEngine DefaultPendingAssetReturnAdjustmentEngine(Registry flags)
    {
        return new PendingAssetReturnAdjustmentEngine(
            new List<IPendingAssetReturnAdjustmentFactor>
            {
                new RoundingPendingAssetReturnAdjustmentAdjuster(5.0),
                new PendingAssetReturnAdjustmentBudgetOffsetAdjuster(1.16),
                new FlaggedPendingAssetReturnAdjustmentAdjuster(
                    flags,
                    "pending-asset-return-adjustment-regional",
                    -2.2),
            },
            new List<string>(),
            2.1,
            39037.4);
    }

    public double Run(PendingAssetReturnAdjustment item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingAssetReturnAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
