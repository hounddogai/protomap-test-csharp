namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional span digest calculation.</summary>
public interface IRegionalSpanDigestFactor
{
    string Describe();
    double Adjust(RegionalSpanDigest item, double valueValue);
}

public sealed record RoundingRegionalSpanDigestAdjuster(double Step) : IRegionalSpanDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSpanDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionalSpanDigestWeightOffsetAdjuster(double Weight) : IRegionalSpanDigestFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RegionalSpanDigest item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumRegionalSpanDigestAdjuster(double Minimum) : IRegionalSpanDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSpanDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRegionalSpanDigestAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSpanDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSpanDigest item, double valueValue)
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

/// <summary>Runs the regional span digest steps in order and keeps a trace of each.</summary>
public sealed class RegionalSpanDigestEngine(List<IRegionalSpanDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSpanDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSpanDigestEngine DefaultRegionalSpanDigestEngine()
    {
        return new RegionalSpanDigestEngine(
            new List<IRegionalSpanDigestFactor>
            {
                new RoundingRegionalSpanDigestAdjuster(5.0),
                new RegionalSpanDigestWeightOffsetAdjuster(1.85),
                new MinimumRegionalSpanDigestAdjuster(31.9),
                new TieredRegionalSpanDigestAdjuster(
                    new List<double> { 1236.0, 6272.0, 7565.0 },
                    new List<double> { 0.81, 1.16, 1.18 }),
            },
            new List<string>(),
            1.3,
            39900.1);
    }

    public double Compute(RegionalSpanDigest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSpanDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
