namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy asset return rule calculation.</summary>
public interface ILegacyAssetReturnRuleModifier
{
    string Describe();
    double Adjust(LegacyAssetReturnRule item, double valueValue);
}

public sealed record PercentLegacyAssetReturnRuleAdjuster(double Percent) : ILegacyAssetReturnRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyAssetReturnRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyAssetReturnRuleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyAssetReturnRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyAssetReturnRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyAssetReturnRuleAdjuster(double Cap) : ILegacyAssetReturnRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyAssetReturnRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceLegacyAssetReturnRuleAdjuster(Dictionary<string, double> Factors) : ILegacyAssetReturnRuleModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LegacyAssetReturnRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacyAssetReturnRuleAttemptsOffsetAdjuster(double Weight) : ILegacyAssetReturnRuleModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LegacyAssetReturnRule item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

/// <summary>Runs the legacy asset return rule steps in order and keeps a trace of each.</summary>
public sealed class LegacyAssetReturnRuleEngine(List<ILegacyAssetReturnRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAssetReturnRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAssetReturnRuleEngine DefaultLegacyAssetReturnRuleEngine(Registry flags)
    {
        return new LegacyAssetReturnRuleEngine(
            new List<ILegacyAssetReturnRuleModifier>
            {
                new PercentLegacyAssetReturnRuleAdjuster(-22.9),
                new FlaggedLegacyAssetReturnRuleAdjuster(flags, "legacy-asset-return-rule-strict", -5.0),
                new CapLegacyAssetReturnRuleAdjuster(45868.0),
                new SourceLegacyAssetReturnRuleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.92, ["north"] = 0.74, ["west"] = 1.13 }),
                new LegacyAssetReturnRuleAttemptsOffsetAdjuster(0.15),
            },
            new List<string>(),
            3.6,
            76669.5);
    }

    public double Compute(LegacyAssetReturnRule item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAssetReturnRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
