namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy cost model manifest calculation.</summary>
public interface ILegacyCostModelManifestModifier
{
    string Describe();
    double Adjust(LegacyCostModelManifest item, double valueValue);
}

public sealed record RoundingLegacyCostModelManifestAdjuster(double Step) : ILegacyCostModelManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyCostModelManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LegacyCostModelManifestCostOffsetAdjuster(double Weight) : ILegacyCostModelManifestModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LegacyCostModelManifest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record PercentLegacyCostModelManifestAdjuster(double Percent) : ILegacyCostModelManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyCostModelManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy cost model manifest steps in order and keeps a trace of each.</summary>
public sealed class LegacyCostModelManifestEngine(List<ILegacyCostModelManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyCostModelManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyCostModelManifestEngine DefaultLegacyCostModelManifestEngine()
    {
        return new LegacyCostModelManifestEngine(
            new List<ILegacyCostModelManifestModifier>
            {
                new RoundingLegacyCostModelManifestAdjuster(5.0),
                new LegacyCostModelManifestCostOffsetAdjuster(1.51),
                new PercentLegacyCostModelManifestAdjuster(7.4),
            },
            new List<string>(),
            0.3,
            55058.0);
    }

    public double Run(LegacyCostModelManifest item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyCostModelManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
