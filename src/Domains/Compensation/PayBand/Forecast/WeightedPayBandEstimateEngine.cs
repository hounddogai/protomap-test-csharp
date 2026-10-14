namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted pay band estimate calculation.</summary>
public interface IWeightedPayBandEstimateModifier
{
    string Describe();
    double Adjust(WeightedPayBandEstimate item, double valueValue);
}

public sealed record RoundingWeightedPayBandEstimateAdjuster(double Step) : IWeightedPayBandEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedPayBandEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedPayBandEstimateFteOffsetAdjuster(double Weight) : IWeightedPayBandEstimateModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(WeightedPayBandEstimate item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record PercentWeightedPayBandEstimateAdjuster(double Percent) : IWeightedPayBandEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPayBandEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedPayBandEstimateAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPayBandEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPayBandEstimate item, double valueValue)
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

/// <summary>Runs the weighted pay band estimate steps in order and keeps a trace of each.</summary>
public sealed class WeightedPayBandEstimateEngine(List<IWeightedPayBandEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPayBandEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPayBandEstimateEngine DefaultWeightedPayBandEstimateEngine()
    {
        return new WeightedPayBandEstimateEngine(
            new List<IWeightedPayBandEstimateModifier>
            {
                new RoundingWeightedPayBandEstimateAdjuster(0.05),
                new WeightedPayBandEstimateFteOffsetAdjuster(1.93),
                new PercentWeightedPayBandEstimateAdjuster(7.8),
                new TieredWeightedPayBandEstimateAdjuster(
                    new List<double> { 7711.0, 8390.0 },
                    new List<double> { 1.03, 1.1 }),
            },
            new List<string>(),
            4.1,
            29127.9);
    }

    public double Evaluate(WeightedPayBandEstimate item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPayBandEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
