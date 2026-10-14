namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy desk manifest calculation.</summary>
public interface ILegacyDeskManifestFactor
{
    string Describe();
    double Adjust(LegacyDeskManifest item, double valueValue);
}

public sealed record CapLegacyDeskManifestAdjuster(double Cap) : ILegacyDeskManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyDeskManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyDeskManifestAdjuster(double Step) : ILegacyDeskManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyDeskManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyDeskManifestAdjuster(double Percent) : ILegacyDeskManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyDeskManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentLegacyDeskManifestAdjuster(Dictionary<string, double> Factors) : ILegacyDeskManifestFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LegacyDeskManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyDeskManifestAdjuster(List<double> Bounds, List<double> Rates) : ILegacyDeskManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyDeskManifest item, double valueValue)
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

/// <summary>Runs the legacy desk manifest steps in order and keeps a trace of each.</summary>
public sealed class LegacyDeskManifestEngine(List<ILegacyDeskManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyDeskManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyDeskManifestEngine DefaultLegacyDeskManifestEngine()
    {
        return new LegacyDeskManifestEngine(
            new List<ILegacyDeskManifestFactor>
            {
                new CapLegacyDeskManifestAdjuster(45636.0),
                new RoundingLegacyDeskManifestAdjuster(1.0),
                new PercentLegacyDeskManifestAdjuster(4.6),
                new SegmentLegacyDeskManifestAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.99, ["north"] = 1.28, ["south"] = 1.23 }),
                new TieredLegacyDeskManifestAdjuster(
                    new List<double> { 1926.0, 3457.0, 3672.0 },
                    new List<double> { 0.91, 0.81, 0.92 }),
            },
            new List<string>(),
            1.3,
            47211.5);
    }

    public double Price(LegacyDeskManifest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyDeskManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
