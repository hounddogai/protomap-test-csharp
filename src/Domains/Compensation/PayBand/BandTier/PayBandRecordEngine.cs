namespace Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>One step of the pay band record calculation.</summary>
public interface IPayBandRecordFactor
{
    string Describe();
    double Adjust(PayBandRecord item, double valueValue);
}

public sealed record PercentPayBandRecordAdjuster(double Percent) : IPayBandRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayBandRecordAmountOffsetAdjuster(double Weight) : IPayBandRecordFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(PayBandRecord item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredPayBandRecordAdjuster(List<double> Bounds, List<double> Rates) : IPayBandRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandRecord item, double valueValue)
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

/// <summary>Runs the pay band record steps in order and keeps a trace of each.</summary>
public sealed class PayBandRecordEngine(List<IPayBandRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandRecordEngine DefaultPayBandRecordEngine()
    {
        return new PayBandRecordEngine(
            new List<IPayBandRecordFactor>
            {
                new PercentPayBandRecordAdjuster(20.4),
                new PayBandRecordAmountOffsetAdjuster(1.81),
                new TieredPayBandRecordAdjuster(
                    new List<double> { 2924.0, 4095.0, 7676.0 },
                    new List<double> { 1.09, 0.85, 1.14 }),
            },
            new List<string>(),
            1.0,
            24122.1);
    }

    public double Evaluate(PayBandRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
