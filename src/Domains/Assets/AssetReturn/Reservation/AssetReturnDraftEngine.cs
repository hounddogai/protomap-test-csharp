namespace Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>One step of the asset return draft calculation.</summary>
public interface IAssetReturnDraftFactor
{
    string Describe();
    double Adjust(AssetReturnDraft item, double valueValue);
}

public sealed record MinimumAssetReturnDraftAdjuster(double Minimum) : IAssetReturnDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameAssetReturnDraftAdjuster(Dictionary<string, double> Factors) : IAssetReturnDraftFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AssetReturnDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAssetReturnDraftAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnDraft item, double valueValue)
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

/// <summary>Runs the asset return draft steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnDraftEngine(List<IAssetReturnDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnDraftEngine DefaultAssetReturnDraftEngine()
    {
        return new AssetReturnDraftEngine(
            new List<IAssetReturnDraftFactor>
            {
                new MinimumAssetReturnDraftAdjuster(44.3),
                new NameAssetReturnDraftAdjuster(new Dictionary<string, double> { ["west"] = 1.4, ["east"] = 1.19 }),
                new TieredAssetReturnDraftAdjuster(
                    new List<double> { 2066.0, 4894.0 },
                    new List<double> { 0.97, 0.94 }),
            },
            new List<string>(),
            1.6,
            43290.4);
    }

    public double Run(AssetReturnDraft item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
