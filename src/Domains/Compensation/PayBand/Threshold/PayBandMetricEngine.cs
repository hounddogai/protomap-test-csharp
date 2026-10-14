namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band metric calculation.</summary>
public interface IPayBandMetricAdjuster
{
    string Describe();
    double Adjust(PayBandMetric item, double valueValue);
}

public sealed record CapPayBandMetricAdjuster(double Cap) : IPayBandMetricAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPayBandMetricAdjuster(double Step) : IPayBandMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPayBandMetricAdjuster(double Percent) : IPayBandMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayBandMetricMarginOffsetAdjuster(double Weight) : IPayBandMetricAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PayBandMetric item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record SegmentPayBandMetricAdjuster(Dictionary<string, double> Factors) : IPayBandMetricAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PayBandMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayBandMetricAdjuster(List<double> Bounds, List<double> Rates) : IPayBandMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandMetric item, double valueValue)
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

/// <summary>Runs the pay band metric steps in order and keeps a trace of each.</summary>
public sealed class PayBandMetricEngine(List<IPayBandMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandMetricEngine DefaultPayBandMetricEngine()
    {
        return new PayBandMetricEngine(
            new List<IPayBandMetricAdjuster>
            {
                new CapPayBandMetricAdjuster(41503.0),
                new RoundingPayBandMetricAdjuster(5.0),
                new PercentPayBandMetricAdjuster(-4.4),
                new PayBandMetricMarginOffsetAdjuster(1.16),
                new SegmentPayBandMetricAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.26, ["retail"] = 1.04, ["online"] = 1.38 }),
                new TieredPayBandMetricAdjuster(
                    new List<double> { 370.0, 3897.0, 6590.0, 8178.0 },
                    new List<double> { 0.96, 1.04, 1.15, 1.02 }),
            },
            new List<string>(),
            4.5,
            57680.7);
    }

    public double Price(PayBandMetric item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
