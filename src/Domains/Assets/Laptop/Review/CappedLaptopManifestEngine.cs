namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped laptop manifest calculation.</summary>
public interface ICappedLaptopManifestFactor
{
    string Describe();
    double Adjust(CappedLaptopManifest item, double valueValue);
}

public sealed record TieredCappedLaptopManifestAdjuster(List<double> Bounds, List<double> Rates) : ICappedLaptopManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedLaptopManifest item, double valueValue)
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

public sealed record NameCappedLaptopManifestAdjuster(Dictionary<string, double> Factors) : ICappedLaptopManifestFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CappedLaptopManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedLaptopManifestAdjuster(double Step) : ICappedLaptopManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedLaptopManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedLaptopManifestUpdatedAtOffsetAdjuster(double Weight) : ICappedLaptopManifestFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(CappedLaptopManifest item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentCappedLaptopManifestAdjuster(double Percent) : ICappedLaptopManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedLaptopManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedLaptopManifestAdjuster(Registry Flags, string Flag, double Percent) : ICappedLaptopManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedLaptopManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped laptop manifest steps in order and keeps a trace of each.</summary>
public sealed class CappedLaptopManifestEngine(List<ICappedLaptopManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedLaptopManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedLaptopManifestEngine DefaultCappedLaptopManifestEngine(Registry flags)
    {
        return new CappedLaptopManifestEngine(
            new List<ICappedLaptopManifestFactor>
            {
                new TieredCappedLaptopManifestAdjuster(
                    new List<double> { 4613.0, 5232.0 },
                    new List<double> { 1.17, 1.03 }),
                new NameCappedLaptopManifestAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.1, ["east"] = 1.12, ["south"] = 1.22 }),
                new RoundingCappedLaptopManifestAdjuster(0.25),
                new CappedLaptopManifestUpdatedAtOffsetAdjuster(1.53),
                new PercentCappedLaptopManifestAdjuster(-18.4),
                new FlaggedCappedLaptopManifestAdjuster(flags, "capped-laptop-manifest-weighted", -1.5),
            },
            new List<string>(),
            4.4,
            88771.1);
    }

    public double Price(CappedLaptopManifest item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedLaptopManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
