namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary pay band manifest calculation.</summary>
public interface IPrimaryPayBandManifestFactor
{
    string Describe();
    double Adjust(PrimaryPayBandManifest item, double valueValue);
}

public sealed record TitlePrimaryPayBandManifestAdjuster(Dictionary<string, double> Factors) : IPrimaryPayBandManifestFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryPayBandManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryPayBandManifestAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryPayBandManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryPayBandManifest item, double valueValue)
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

public sealed record FlaggedPrimaryPayBandManifestAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPayBandManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPayBandManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryPayBandManifestAdjuster(double Step) : IPrimaryPayBandManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryPayBandManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PrimaryPayBandManifestQuantityOffsetAdjuster(double Weight) : IPrimaryPayBandManifestFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(PrimaryPayBandManifest item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the primary pay band manifest steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPayBandManifestEngine(List<IPrimaryPayBandManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPayBandManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPayBandManifestEngine DefaultPrimaryPayBandManifestEngine(Registry flags)
    {
        return new PrimaryPayBandManifestEngine(
            new List<IPrimaryPayBandManifestFactor>
            {
                new TitlePrimaryPayBandManifestAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.14, ["south"] = 1.3, ["north"] = 0.77 }),
                new TieredPrimaryPayBandManifestAdjuster(
                    new List<double> { 3655.0, 7246.0 },
                    new List<double> { 1.18, 1.07 }),
                new FlaggedPrimaryPayBandManifestAdjuster(flags, "primary-pay-band-manifest-batched", -8.9),
                new RoundingPrimaryPayBandManifestAdjuster(1.0),
                new PrimaryPayBandManifestQuantityOffsetAdjuster(0.07),
            },
            new List<string>(),
            2.6,
            13336.2);
    }

    public double Compute(PrimaryPayBandManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPayBandManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
