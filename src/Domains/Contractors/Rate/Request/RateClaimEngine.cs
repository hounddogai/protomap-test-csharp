namespace Acme.Workforce.Contractors.Rate.Request;

/// <summary>One step of the rate claim calculation.</summary>
public interface IRateClaimModifier
{
    string Describe();
    double Adjust(RateClaim item, double valueValue);
}

public sealed record CapRateClaimAdjuster(double Cap) : IRateClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRateClaimAdjuster(double Percent) : IRateClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRateClaimAdjuster(List<double> Bounds, List<double> Rates) : IRateClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateClaim item, double valueValue)
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

/// <summary>Runs the rate claim steps in order and keeps a trace of each.</summary>
public sealed class RateClaimEngine(List<IRateClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateClaimEngine DefaultRateClaimEngine()
    {
        return new RateClaimEngine(
            new List<IRateClaimModifier>
            {
                new CapRateClaimAdjuster(21947.0),
                new PercentRateClaimAdjuster(-18.5),
                new TieredRateClaimAdjuster(
                    new List<double> { 554.0, 1131.0, 3068.0, 8990.0 },
                    new List<double> { 1.06, 0.83, 0.87, 0.94 }),
            },
            new List<string>(),
            3.9,
            18016.3);
    }

    public double Evaluate(RateClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RateClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
