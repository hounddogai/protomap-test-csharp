namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate variance calculation.</summary>
public interface IRateVarianceAdjuster
{
    string Describe();
    double Adjust(RateVariance item, double valueValue);
}

public sealed record MinimumRateVarianceAdjuster(double Minimum) : IRateVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RateVarianceWeightOffsetAdjuster(double Weight) : IRateVarianceAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RateVariance item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingRateVarianceAdjuster(double Step) : IRateVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRateVarianceAdjuster(double Cap) : IRateVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRateVarianceAdjuster(List<double> Bounds, List<double> Rates) : IRateVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateVariance item, double valueValue)
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

public sealed record PercentRateVarianceAdjuster(double Percent) : IRateVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate variance steps in order and keeps a trace of each.</summary>
public sealed class RateVarianceEngine(List<IRateVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateVarianceEngine DefaultRateVarianceEngine()
    {
        return new RateVarianceEngine(
            new List<IRateVarianceAdjuster>
            {
                new MinimumRateVarianceAdjuster(6.9),
                new RateVarianceWeightOffsetAdjuster(1.48),
                new RoundingRateVarianceAdjuster(0.5),
                new CapRateVarianceAdjuster(48785.0),
                new TieredRateVarianceAdjuster(
                    new List<double> { 1245.0, 2600.0, 6262.0, 8137.0 },
                    new List<double> { 0.91, 0.91, 0.84, 1.06 }),
                new PercentRateVarianceAdjuster(6.2),
            },
            new List<string>(),
            1.7,
            96502.9);
    }

    public double Compute(RateVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<RateVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
