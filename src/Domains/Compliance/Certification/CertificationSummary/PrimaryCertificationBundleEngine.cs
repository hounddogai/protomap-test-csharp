namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary certification bundle calculation.</summary>
public interface IPrimaryCertificationBundleAdjuster
{
    string Describe();
    double Adjust(PrimaryCertificationBundle item, double valueValue);
}

public sealed record FlaggedPrimaryCertificationBundleAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryCertificationBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryCertificationBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryCertificationBundleAdjuster(double Step) : IPrimaryCertificationBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryCertificationBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPrimaryCertificationBundleAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryCertificationBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryCertificationBundle item, double valueValue)
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

public sealed record CountryPrimaryCertificationBundleAdjuster(Dictionary<string, double> Factors) : IPrimaryCertificationBundleAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PrimaryCertificationBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PrimaryCertificationBundleUpdatedAtOffsetAdjuster(double Weight) : IPrimaryCertificationBundleAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(PrimaryCertificationBundle item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the primary certification bundle steps in order and keeps a trace of each.</summary>
public sealed class PrimaryCertificationBundleEngine(List<IPrimaryCertificationBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryCertificationBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryCertificationBundleEngine DefaultPrimaryCertificationBundleEngine(Registry flags)
    {
        return new PrimaryCertificationBundleEngine(
            new List<IPrimaryCertificationBundleAdjuster>
            {
                new FlaggedPrimaryCertificationBundleAdjuster(flags, "primary-certification-bundle-fallback", 8.1),
                new RoundingPrimaryCertificationBundleAdjuster(1.0),
                new TieredPrimaryCertificationBundleAdjuster(
                    new List<double> { 1792.0, 3417.0, 6192.0 },
                    new List<double> { 0.86, 0.99, 0.86 }),
                new CountryPrimaryCertificationBundleAdjuster(new Dictionary<string, double> { ["west"] = 0.89 }),
                new PrimaryCertificationBundleUpdatedAtOffsetAdjuster(1.83),
            },
            new List<string>(),
            3.8,
            37545.5);
    }

    public double Run(PrimaryCertificationBundle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryCertificationBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
