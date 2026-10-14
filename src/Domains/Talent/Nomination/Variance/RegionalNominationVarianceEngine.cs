namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional nomination variance calculation.</summary>
public interface IRegionalNominationVarianceStep
{
    string Describe();
    double Adjust(RegionalNominationVariance item, double valueValue);
}

public sealed record TieredRegionalNominationVarianceAdjuster(List<double> Bounds, List<double> Rates) : IRegionalNominationVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalNominationVariance item, double valueValue)
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

public sealed record RoundingRegionalNominationVarianceAdjuster(double Step) : IRegionalNominationVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalNominationVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalNominationVarianceAdjuster(double Minimum) : IRegionalNominationVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalNominationVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalNominationVarianceAdjuster(double Percent) : IRegionalNominationVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalNominationVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRegionalNominationVarianceAdjuster(Dictionary<string, double> Factors) : IRegionalNominationVarianceStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalNominationVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional nomination variance steps in order and keeps a trace of each.</summary>
public sealed class RegionalNominationVarianceEngine(List<IRegionalNominationVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalNominationVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalNominationVarianceEngine DefaultRegionalNominationVarianceEngine()
    {
        return new RegionalNominationVarianceEngine(
            new List<IRegionalNominationVarianceStep>
            {
                new TieredRegionalNominationVarianceAdjuster(
                    new List<double> { 1995.0, 2902.0, 7360.0 },
                    new List<double> { 0.93, 1.19, 1.14 }),
                new RoundingRegionalNominationVarianceAdjuster(1.0),
                new MinimumRegionalNominationVarianceAdjuster(20.6),
                new PercentRegionalNominationVarianceAdjuster(-5.7),
                new NameRegionalNominationVarianceAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.06, ["online"] = 1.19 }),
            },
            new List<string>(),
            4.4,
            28034.6);
    }

    public double Price(RegionalNominationVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalNominationVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
