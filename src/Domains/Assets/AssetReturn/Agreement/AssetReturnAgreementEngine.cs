namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the asset return agreement calculation.</summary>
public interface IAssetReturnAgreementAdjuster
{
    string Describe();
    double Adjust(AssetReturnAgreement item, double valueValue);
}

public sealed record TieredAssetReturnAgreementAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnAgreement item, double valueValue)
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

public sealed record PercentAssetReturnAgreementAdjuster(double Percent) : IAssetReturnAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAssetReturnAgreementAdjuster(double Minimum) : IAssetReturnAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAssetReturnAgreementAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return agreement steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnAgreementEngine(List<IAssetReturnAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnAgreementEngine DefaultAssetReturnAgreementEngine(Registry flags)
    {
        return new AssetReturnAgreementEngine(
            new List<IAssetReturnAgreementAdjuster>
            {
                new TieredAssetReturnAgreementAdjuster(
                    new List<double> { 1585.0, 4274.0, 8471.0 },
                    new List<double> { 0.9, 0.84, 0.91 }),
                new PercentAssetReturnAgreementAdjuster(-16.1),
                new MinimumAssetReturnAgreementAdjuster(6.9),
                new FlaggedAssetReturnAgreementAdjuster(flags, "asset-return-agreement-global", -5.0),
            },
            new List<string>(),
            1.8,
            48212.5);
    }

    public double Price(AssetReturnAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
