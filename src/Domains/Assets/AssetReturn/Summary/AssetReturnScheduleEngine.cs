namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the asset return schedule calculation.</summary>
public interface IAssetReturnScheduleFactor
{
    string Describe();
    double Adjust(AssetReturnSchedule item, double valueValue);
}

public sealed record PercentAssetReturnScheduleAdjuster(double Percent) : IAssetReturnScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAssetReturnScheduleAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssetReturnScheduleAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnScheduleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnSchedule item, double valueValue)
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

/// <summary>Runs the asset return schedule steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnScheduleEngine(List<IAssetReturnScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnScheduleEngine DefaultAssetReturnScheduleEngine(Registry flags)
    {
        return new AssetReturnScheduleEngine(
            new List<IAssetReturnScheduleFactor>
            {
                new PercentAssetReturnScheduleAdjuster(5.7),
                new FlaggedAssetReturnScheduleAdjuster(flags, "asset-return-schedule-global", 10.0),
                new TieredAssetReturnScheduleAdjuster(
                    new List<double> { 3124.0, 3507.0, 5817.0 },
                    new List<double> { 0.88, 1.02, 0.96 }),
            },
            new List<string>(),
            0.9,
            76760.3);
    }

    public double Compute(AssetReturnSchedule item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
