namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pool variance calculation.</summary>
public interface ILegacyPoolVarianceFactor
{
    string Describe();
    double Adjust(LegacyPoolVariance item, double valueValue);
}

public sealed record CapLegacyPoolVarianceAdjuster(double Cap) : ILegacyPoolVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPoolVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyPoolVarianceMinimumOffsetAdjuster(double Weight) : ILegacyPoolVarianceFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(LegacyPoolVariance item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingLegacyPoolVarianceAdjuster(double Step) : ILegacyPoolVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPoolVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy pool variance steps in order and keeps a trace of each.</summary>
public sealed class LegacyPoolVarianceEngine(List<ILegacyPoolVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPoolVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPoolVarianceEngine DefaultLegacyPoolVarianceEngine()
    {
        return new LegacyPoolVarianceEngine(
            new List<ILegacyPoolVarianceFactor>
            {
                new CapLegacyPoolVarianceAdjuster(40315.0),
                new LegacyPoolVarianceMinimumOffsetAdjuster(0.62),
                new RoundingLegacyPoolVarianceAdjuster(0.05),
            },
            new List<string>(),
            0.6,
            29898.4);
    }

    public double Run(LegacyPoolVariance item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPoolVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
