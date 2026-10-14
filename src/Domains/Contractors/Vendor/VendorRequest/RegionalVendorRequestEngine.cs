namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional vendor request calculation.</summary>
public interface IRegionalVendorRequestStep
{
    string Describe();
    double Adjust(RegionalVendorRequest item, double valueValue);
}

public sealed record RoundingRegionalVendorRequestAdjuster(double Step) : IRegionalVendorRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalVendorRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalVendorRequestAdjuster(double Percent) : IRegionalVendorRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalVendorRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalVendorRequestAdjuster(Registry Flags, string Flag, double Percent) : IRegionalVendorRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalVendorRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalVendorRequestScoreOffsetAdjuster(double Weight) : IRegionalVendorRequestStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RegionalVendorRequest item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredRegionalVendorRequestAdjuster(List<double> Bounds, List<double> Rates) : IRegionalVendorRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalVendorRequest item, double valueValue)
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

public sealed record CountryRegionalVendorRequestAdjuster(Dictionary<string, double> Factors) : IRegionalVendorRequestStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RegionalVendorRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional vendor request steps in order and keeps a trace of each.</summary>
public sealed class RegionalVendorRequestEngine(List<IRegionalVendorRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalVendorRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalVendorRequestEngine DefaultRegionalVendorRequestEngine(Registry flags)
    {
        return new RegionalVendorRequestEngine(
            new List<IRegionalVendorRequestStep>
            {
                new RoundingRegionalVendorRequestAdjuster(5.0),
                new PercentRegionalVendorRequestAdjuster(-3.5),
                new FlaggedRegionalVendorRequestAdjuster(flags, "regional-vendor-request-strict", 1.7),
                new RegionalVendorRequestScoreOffsetAdjuster(1.56),
                new TieredRegionalVendorRequestAdjuster(
                    new List<double> { 5215.0, 6958.0, 8039.0, 9837.0 },
                    new List<double> { 0.97, 1.0, 0.98, 0.93 }),
                new CountryRegionalVendorRequestAdjuster(new Dictionary<string, double> { ["online"] = 1.08 }),
            },
            new List<string>(),
            2.1,
            12961.2);
    }

    public double Price(RegionalVendorRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalVendorRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
