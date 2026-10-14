namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return bundle calculation.</summary>
public interface IAssetReturnBundleAdjuster
{
    string Describe();
    double Adjust(AssetReturnBundle item, double valueValue);
}

public sealed record DepartmentAssetReturnBundleAdjuster(Dictionary<string, double> Factors) : IAssetReturnBundleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AssetReturnBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAssetReturnBundleAdjuster(double Cap) : IAssetReturnBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAssetReturnBundleAdjuster(double Percent) : IAssetReturnBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return bundle steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnBundleEngine(List<IAssetReturnBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnBundleEngine DefaultAssetReturnBundleEngine()
    {
        return new AssetReturnBundleEngine(
            new List<IAssetReturnBundleAdjuster>
            {
                new DepartmentAssetReturnBundleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.75, ["south"] = 1.37, ["east"] = 1.03 }),
                new CapAssetReturnBundleAdjuster(10445.0),
                new PercentAssetReturnBundleAdjuster(-4.7),
            },
            new List<string>(),
            4.2,
            25632.1);
    }

    public double Run(AssetReturnBundle item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
