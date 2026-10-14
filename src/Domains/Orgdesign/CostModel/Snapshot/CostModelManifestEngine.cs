namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model manifest calculation.</summary>
public interface ICostModelManifestFactor
{
    string Describe();
    double Adjust(CostModelManifest item, double valueValue);
}

public sealed record CostModelManifestExpiresAtOffsetAdjuster(double Weight) : ICostModelManifestFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(CostModelManifest item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentCostModelManifestAdjuster(double Percent) : ICostModelManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCostModelManifestAdjuster(double Minimum) : ICostModelManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCostModelManifestAdjuster(List<double> Bounds, List<double> Rates) : ICostModelManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelManifest item, double valueValue)
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

public sealed record RoundingCostModelManifestAdjuster(double Step) : ICostModelManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCostModelManifestAdjuster(Registry Flags, string Flag, double Percent) : ICostModelManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model manifest steps in order and keeps a trace of each.</summary>
public sealed class CostModelManifestEngine(List<ICostModelManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelManifestEngine DefaultCostModelManifestEngine(Registry flags)
    {
        return new CostModelManifestEngine(
            new List<ICostModelManifestFactor>
            {
                new CostModelManifestExpiresAtOffsetAdjuster(0.46),
                new PercentCostModelManifestAdjuster(4.4),
                new MinimumCostModelManifestAdjuster(42.2),
                new TieredCostModelManifestAdjuster(
                    new List<double> { 5707.0, 5788.0, 7201.0 },
                    new List<double> { 1.18, 1.06, 0.96 }),
                new RoundingCostModelManifestAdjuster(0.5),
                new FlaggedCostModelManifestAdjuster(flags, "cost-model-manifest-global", 9.0),
            },
            new List<string>(),
            2.0,
            32636.8);
    }

    public double Price(CostModelManifest item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
