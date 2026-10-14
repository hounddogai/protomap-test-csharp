namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive laptop manifest calculation.</summary>
public interface IAdaptiveLaptopManifestAdjuster
{
    string Describe();
    double Adjust(AdaptiveLaptopManifest item, double valueValue);
}

public sealed record AdaptiveLaptopManifestFloorValueOffsetAdjuster(double Weight) : IAdaptiveLaptopManifestAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(AdaptiveLaptopManifest item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumAdaptiveLaptopManifestAdjuster(double Minimum) : IAdaptiveLaptopManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveLaptopManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAdaptiveLaptopManifestAdjuster(double Step) : IAdaptiveLaptopManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveLaptopManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAdaptiveLaptopManifestAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveLaptopManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveLaptopManifest item, double valueValue)
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

public sealed record SourceAdaptiveLaptopManifestAdjuster(Dictionary<string, double> Factors) : IAdaptiveLaptopManifestAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdaptiveLaptopManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive laptop manifest steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveLaptopManifestEngine(List<IAdaptiveLaptopManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveLaptopManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveLaptopManifestEngine DefaultAdaptiveLaptopManifestEngine()
    {
        return new AdaptiveLaptopManifestEngine(
            new List<IAdaptiveLaptopManifestAdjuster>
            {
                new AdaptiveLaptopManifestFloorValueOffsetAdjuster(1.49),
                new MinimumAdaptiveLaptopManifestAdjuster(36.6),
                new RoundingAdaptiveLaptopManifestAdjuster(0.1),
                new TieredAdaptiveLaptopManifestAdjuster(
                    new List<double> { 4653.0, 5900.0, 7545.0 },
                    new List<double> { 1.09, 1.02, 1.03 }),
                new SourceAdaptiveLaptopManifestAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.19, ["north"] = 1.11 }),
            },
            new List<string>(),
            4.2,
            62419.8);
    }

    public double Price(AdaptiveLaptopManifest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveLaptopManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
