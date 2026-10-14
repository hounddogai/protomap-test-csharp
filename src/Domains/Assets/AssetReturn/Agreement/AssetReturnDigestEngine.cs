namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return digest calculation.</summary>
public interface IAssetReturnDigestStep
{
    string Describe();
    double Adjust(AssetReturnDigest item, double valueValue);
}

public sealed record AssetReturnDigestPriorityOffsetAdjuster(double Weight) : IAssetReturnDigestStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(AssetReturnDigest item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record LabelAssetReturnDigestAdjuster(Dictionary<string, double> Factors) : IAssetReturnDigestStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AssetReturnDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAssetReturnDigestAdjuster(double Cap) : IAssetReturnDigestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAssetReturnDigestAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssetReturnDigestAdjuster(double Minimum) : IAssetReturnDigestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the asset return digest steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnDigestEngine(List<IAssetReturnDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnDigestEngine DefaultAssetReturnDigestEngine(Registry flags)
    {
        return new AssetReturnDigestEngine(
            new List<IAssetReturnDigestStep>
            {
                new AssetReturnDigestPriorityOffsetAdjuster(1.88),
                new LabelAssetReturnDigestAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.96, ["east"] = 1.09, ["central"] = 0.82 }),
                new CapAssetReturnDigestAdjuster(850.0),
                new FlaggedAssetReturnDigestAdjuster(flags, "asset-return-digest-primary", 6.8),
                new MinimumAssetReturnDigestAdjuster(0.1),
            },
            new List<string>(),
            0.2,
            90187.3);
    }

    public double Run(AssetReturnDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
