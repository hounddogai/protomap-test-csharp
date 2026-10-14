namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional pay run variance calculation.</summary>
public interface IRegionalPayRunVarianceModifier
{
    string Describe();
    double Adjust(RegionalPayRunVariance item, double valueValue);
}

public sealed record TieredRegionalPayRunVarianceAdjuster(List<double> Bounds, List<double> Rates) : IRegionalPayRunVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalPayRunVariance item, double valueValue)
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

public sealed record RoundingRegionalPayRunVarianceAdjuster(double Step) : IRegionalPayRunVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPayRunVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryRegionalPayRunVarianceAdjuster(Dictionary<string, double> Factors) : IRegionalPayRunVarianceModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RegionalPayRunVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional pay run variance steps in order and keeps a trace of each.</summary>
public sealed class RegionalPayRunVarianceEngine(List<IRegionalPayRunVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPayRunVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPayRunVarianceEngine DefaultRegionalPayRunVarianceEngine()
    {
        return new RegionalPayRunVarianceEngine(
            new List<IRegionalPayRunVarianceModifier>
            {
                new TieredRegionalPayRunVarianceAdjuster(
                    new List<double> { 5796.0, 7201.0, 9492.0 },
                    new List<double> { 0.89, 1.15, 0.87 }),
                new RoundingRegionalPayRunVarianceAdjuster(5.0),
                new CountryRegionalPayRunVarianceAdjuster(new Dictionary<string, double> { ["north"] = 0.86 }),
            },
            new List<string>(),
            3.9,
            17320.5);
    }

    public double Run(RegionalPayRunVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPayRunVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
