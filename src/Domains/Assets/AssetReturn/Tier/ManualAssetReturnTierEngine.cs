namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual asset return tier calculation.</summary>
public interface IManualAssetReturnTierAdjuster
{
    string Describe();
    double Adjust(ManualAssetReturnTier item, double valueValue);
}

public sealed record TitleManualAssetReturnTierAdjuster(Dictionary<string, double> Factors) : IManualAssetReturnTierAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ManualAssetReturnTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapManualAssetReturnTierAdjuster(double Cap) : IManualAssetReturnTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAssetReturnTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualAssetReturnTierAdjuster(double Step) : IManualAssetReturnTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAssetReturnTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ManualAssetReturnTierAmountOffsetAdjuster(double Weight) : IManualAssetReturnTierAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(ManualAssetReturnTier item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the manual asset return tier steps in order and keeps a trace of each.</summary>
public sealed class ManualAssetReturnTierEngine(List<IManualAssetReturnTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAssetReturnTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAssetReturnTierEngine DefaultManualAssetReturnTierEngine()
    {
        return new ManualAssetReturnTierEngine(
            new List<IManualAssetReturnTierAdjuster>
            {
                new TitleManualAssetReturnTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.27, ["central"] = 1.11, ["south"] = 0.97 }),
                new CapManualAssetReturnTierAdjuster(41795.0),
                new RoundingManualAssetReturnTierAdjuster(0.05),
                new ManualAssetReturnTierAmountOffsetAdjuster(0.45),
            },
            new List<string>(),
            2.3,
            69972.6);
    }

    public double Evaluate(ManualAssetReturnTier item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAssetReturnTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
