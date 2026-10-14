namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band digest calculation.</summary>
public interface IPayBandDigestAdjuster
{
    string Describe();
    double Adjust(PayBandDigest item, double valueValue);
}

public sealed record TieredPayBandDigestAdjuster(List<double> Bounds, List<double> Rates) : IPayBandDigestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandDigest item, double valueValue)
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

public sealed record CapPayBandDigestAdjuster(double Cap) : IPayBandDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPayBandDigestAdjuster(double Step) : IPayBandDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pay band digest steps in order and keeps a trace of each.</summary>
public sealed class PayBandDigestEngine(List<IPayBandDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandDigestEngine DefaultPayBandDigestEngine()
    {
        return new PayBandDigestEngine(
            new List<IPayBandDigestAdjuster>
            {
                new TieredPayBandDigestAdjuster(new List<double> { 1822.0, 9422.0 }, new List<double> { 0.8, 1.01 }),
                new CapPayBandDigestAdjuster(1867.0),
                new RoundingPayBandDigestAdjuster(5.0),
            },
            new List<string>(),
            0.7,
            67265.5);
    }

    public double Evaluate(PayBandDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
