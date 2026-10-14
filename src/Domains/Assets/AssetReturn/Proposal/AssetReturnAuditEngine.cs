namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the asset return audit calculation.</summary>
public interface IAssetReturnAuditStep
{
    string Describe();
    double Adjust(AssetReturnAudit item, double valueValue);
}

public sealed record SourceAssetReturnAuditAdjuster(Dictionary<string, double> Factors) : IAssetReturnAuditStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AssetReturnAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAssetReturnAuditAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnAudit item, double valueValue)
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

public sealed record PercentAssetReturnAuditAdjuster(double Percent) : IAssetReturnAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AssetReturnAuditLeaveDaysOffsetAdjuster(double Weight) : IAssetReturnAuditStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AssetReturnAudit item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedAssetReturnAuditAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return audit steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnAuditEngine(List<IAssetReturnAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnAuditEngine DefaultAssetReturnAuditEngine(Registry flags)
    {
        return new AssetReturnAuditEngine(
            new List<IAssetReturnAuditStep>
            {
                new SourceAssetReturnAuditAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.04, ["east"] = 0.93, ["retail"] = 1.12 }),
                new TieredAssetReturnAuditAdjuster(
                    new List<double> { 2699.0, 6876.0 },
                    new List<double> { 1.08, 1.1 }),
                new PercentAssetReturnAuditAdjuster(22.8),
                new AssetReturnAuditLeaveDaysOffsetAdjuster(1.24),
                new FlaggedAssetReturnAuditAdjuster(flags, "asset-return-audit-default", -2.3),
            },
            new List<string>(),
            0.7,
            57113.9);
    }

    public double Price(AssetReturnAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
