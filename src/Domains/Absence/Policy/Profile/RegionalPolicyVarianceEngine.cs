namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional policy variance calculation.</summary>
public interface IRegionalPolicyVarianceAdjuster
{
    string Describe();
    double Adjust(RegionalPolicyVariance item, double valueValue);
}

public sealed record RoundingRegionalPolicyVarianceAdjuster(double Step) : IRegionalPolicyVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPolicyVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalPolicyVarianceAdjuster(List<double> Bounds, List<double> Rates) : IRegionalPolicyVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalPolicyVariance item, double valueValue)
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

public sealed record RegionalPolicyVarianceRateOffsetAdjuster(double Weight) : IRegionalPolicyVarianceAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RegionalPolicyVariance item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record CapRegionalPolicyVarianceAdjuster(double Cap) : IRegionalPolicyVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPolicyVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional policy variance steps in order and keeps a trace of each.</summary>
public sealed class RegionalPolicyVarianceEngine(List<IRegionalPolicyVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPolicyVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPolicyVarianceEngine DefaultRegionalPolicyVarianceEngine()
    {
        return new RegionalPolicyVarianceEngine(
            new List<IRegionalPolicyVarianceAdjuster>
            {
                new RoundingRegionalPolicyVarianceAdjuster(5.0),
                new TieredRegionalPolicyVarianceAdjuster(
                    new List<double> { 918.0, 2464.0, 3238.0, 3552.0 },
                    new List<double> { 0.96, 0.82, 1.15, 0.81 }),
                new RegionalPolicyVarianceRateOffsetAdjuster(1.25),
                new CapRegionalPolicyVarianceAdjuster(29504.0),
            },
            new List<string>(),
            1.2,
            91364.6);
    }

    public double Evaluate(RegionalPolicyVariance item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPolicyVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
