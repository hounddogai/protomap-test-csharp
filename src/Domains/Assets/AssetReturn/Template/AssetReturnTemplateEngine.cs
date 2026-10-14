namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return template calculation.</summary>
public interface IAssetReturnTemplateFactor
{
    string Describe();
    double Adjust(AssetReturnTemplate item, double valueValue);
}

public sealed record TieredAssetReturnTemplateAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnTemplate item, double valueValue)
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

public sealed record AssetReturnTemplateDurationMinutesOffsetAdjuster(double Weight) : IAssetReturnTemplateFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(AssetReturnTemplate item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record FlaggedAssetReturnTemplateAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssetReturnTemplateAdjuster(double Minimum) : IAssetReturnTemplateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentAssetReturnTemplateAdjuster(Dictionary<string, double> Factors) : IAssetReturnTemplateFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(AssetReturnTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the asset return template steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnTemplateEngine(List<IAssetReturnTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnTemplateEngine DefaultAssetReturnTemplateEngine(Registry flags)
    {
        return new AssetReturnTemplateEngine(
            new List<IAssetReturnTemplateFactor>
            {
                new TieredAssetReturnTemplateAdjuster(
                    new List<double> { 2444.0, 6921.0 },
                    new List<double> { 1.03, 1.12 }),
                new AssetReturnTemplateDurationMinutesOffsetAdjuster(0.14),
                new FlaggedAssetReturnTemplateAdjuster(flags, "asset-return-template-weighted", -8.5),
                new MinimumAssetReturnTemplateAdjuster(32.3),
                new SegmentAssetReturnTemplateAdjuster(new Dictionary<string, double> { ["east"] = 0.86 }),
            },
            new List<string>(),
            4.4,
            98116.8);
    }

    public double Compute(AssetReturnTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
