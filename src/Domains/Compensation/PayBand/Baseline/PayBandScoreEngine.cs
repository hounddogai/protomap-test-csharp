namespace Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>One step of the pay band score calculation.</summary>
public interface IPayBandScoreStep
{
    string Describe();
    double Adjust(PayBandScore item, double valueValue);
}

public sealed record TieredPayBandScoreAdjuster(List<double> Bounds, List<double> Rates) : IPayBandScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandScore item, double valueValue)
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

public sealed record MinimumPayBandScoreAdjuster(double Minimum) : IPayBandScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryPayBandScoreAdjuster(Dictionary<string, double> Factors) : IPayBandScoreStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PayBandScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PayBandScoreQuantityOffsetAdjuster(double Weight) : IPayBandScoreStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(PayBandScore item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the pay band score steps in order and keeps a trace of each.</summary>
public sealed class PayBandScoreEngine(List<IPayBandScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandScoreEngine DefaultPayBandScoreEngine()
    {
        return new PayBandScoreEngine(
            new List<IPayBandScoreStep>
            {
                new TieredPayBandScoreAdjuster(
                    new List<double> { 210.0, 2872.0, 4790.0, 6250.0 },
                    new List<double> { 1.07, 0.95, 0.9, 0.83 }),
                new MinimumPayBandScoreAdjuster(42.8),
                new CategoryPayBandScoreAdjuster(new Dictionary<string, double> { ["south"] = 1.16 }),
                new PayBandScoreQuantityOffsetAdjuster(1.29),
            },
            new List<string>(),
            1.9,
            92399.1);
    }

    public double Price(PayBandScore item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
