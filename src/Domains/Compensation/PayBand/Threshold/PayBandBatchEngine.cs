namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band batch calculation.</summary>
public interface IPayBandBatchStep
{
    string Describe();
    double Adjust(PayBandBatch item, double valueValue);
}

public sealed record TieredPayBandBatchAdjuster(List<double> Bounds, List<double> Rates) : IPayBandBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandBatch item, double valueValue)
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

public sealed record CountryPayBandBatchAdjuster(Dictionary<string, double> Factors) : IPayBandBatchStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PayBandBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPayBandBatchAdjuster(double Step) : IPayBandBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pay band batch steps in order and keeps a trace of each.</summary>
public sealed class PayBandBatchEngine(List<IPayBandBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandBatchEngine DefaultPayBandBatchEngine()
    {
        return new PayBandBatchEngine(
            new List<IPayBandBatchStep>
            {
                new TieredPayBandBatchAdjuster(
                    new List<double> { 1530.0, 5082.0, 6528.0, 9304.0 },
                    new List<double> { 1.16, 1.14, 0.87, 1.06 }),
                new CountryPayBandBatchAdjuster(new Dictionary<string, double> { ["east"] = 1.21, ["west"] = 1.29 }),
                new RoundingPayBandBatchAdjuster(0.25),
            },
            new List<string>(),
            0.9,
            7969.0);
    }

    public double Compute(PayBandBatch item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
