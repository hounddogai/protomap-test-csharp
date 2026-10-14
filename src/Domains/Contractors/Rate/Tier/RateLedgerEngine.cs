namespace Acme.Workforce.Contractors.Rate.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate ledger calculation.</summary>
public interface IRateLedgerFactor
{
    string Describe();
    double Adjust(RateLedger item, double valueValue);
}

public sealed record PercentRateLedgerAdjuster(double Percent) : IRateLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRateLedgerAdjuster(double Step) : IRateLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRateLedgerAdjuster(List<double> Bounds, List<double> Rates) : IRateLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateLedger item, double valueValue)
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

/// <summary>Runs the rate ledger steps in order and keeps a trace of each.</summary>
public sealed class RateLedgerEngine(List<IRateLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateLedgerEngine DefaultRateLedgerEngine()
    {
        return new RateLedgerEngine(
            new List<IRateLedgerFactor>
            {
                new PercentRateLedgerAdjuster(-23.8),
                new RoundingRateLedgerAdjuster(0.25),
                new TieredRateLedgerAdjuster(
                    new List<double> { 1474.0, 2010.0, 6621.0, 8384.0 },
                    new List<double> { 1.06, 0.84, 1.09, 1.17 }),
            },
            new List<string>(),
            3.0,
            64258.4);
    }

    public double Compute(RateLedger item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RateLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
