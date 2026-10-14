namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped asset return assignment calculation.</summary>
public interface ICappedAssetReturnAssignmentStep
{
    string Describe();
    double Adjust(CappedAssetReturnAssignment item, double valueValue);
}

public sealed record TieredCappedAssetReturnAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedAssetReturnAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAssetReturnAssignment item, double valueValue)
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

public sealed record RoundingCappedAssetReturnAssignmentAdjuster(double Step) : ICappedAssetReturnAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAssetReturnAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCappedAssetReturnAssignmentAdjuster(double Minimum) : ICappedAssetReturnAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAssetReturnAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedAssetReturnAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedAssetReturnAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAssetReturnAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelCappedAssetReturnAssignmentAdjuster(Dictionary<string, double> Factors) : ICappedAssetReturnAssignmentStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedAssetReturnAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped asset return assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedAssetReturnAssignmentEngine(List<ICappedAssetReturnAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAssetReturnAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAssetReturnAssignmentEngine DefaultCappedAssetReturnAssignmentEngine(Registry flags)
    {
        return new CappedAssetReturnAssignmentEngine(
            new List<ICappedAssetReturnAssignmentStep>
            {
                new TieredCappedAssetReturnAssignmentAdjuster(
                    new List<double> { 2584.0, 2956.0, 3938.0, 6462.0 },
                    new List<double> { 0.99, 0.91, 0.82, 1.01 }),
                new RoundingCappedAssetReturnAssignmentAdjuster(5.0),
                new MinimumCappedAssetReturnAssignmentAdjuster(4.8),
                new FlaggedCappedAssetReturnAssignmentAdjuster(
                    flags,
                    "capped-asset-return-assignment-fallback",
                    -4.2),
                new LabelCappedAssetReturnAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 1.21 }),
            },
            new List<string>(),
            5.0,
            56774.0);
    }

    public double Price(CappedAssetReturnAssignment item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAssetReturnAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
