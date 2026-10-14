namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate quota calculation.</summary>
public interface IRateQuotaModifier
{
    string Describe();
    double Adjust(RateQuota item, double valueValue);
}

public sealed record CapRateQuotaAdjuster(double Cap) : IRateQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRateQuotaAdjuster(double Percent) : IRateQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRateQuotaAdjuster(double Step) : IRateQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRateQuotaAdjuster(List<double> Bounds, List<double> Rates) : IRateQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateQuota item, double valueValue)
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

/// <summary>Runs the rate quota steps in order and keeps a trace of each.</summary>
public sealed class RateQuotaEngine(List<IRateQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateQuotaEngine DefaultRateQuotaEngine()
    {
        return new RateQuotaEngine(
            new List<IRateQuotaModifier>
            {
                new CapRateQuotaAdjuster(6883.0),
                new PercentRateQuotaAdjuster(-13.5),
                new RoundingRateQuotaAdjuster(5.0),
                new TieredRateQuotaAdjuster(new List<double> { 4288.0, 5066.0 }, new List<double> { 1.13, 0.99 }),
            },
            new List<string>(),
            2.7,
            4931.8);
    }

    public double Evaluate(RateQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<RateQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
