namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped rate metric calculation.</summary>
public interface ICappedRateMetricAdjuster
{
    string Describe();
    double Adjust(CappedRateMetric item, double valueValue);
}

public sealed record CappedRateMetricAmountOffsetAdjuster(double Weight) : ICappedRateMetricAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(CappedRateMetric item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredCappedRateMetricAdjuster(List<double> Bounds, List<double> Rates) : ICappedRateMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRateMetric item, double valueValue)
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

public sealed record MinimumCappedRateMetricAdjuster(double Minimum) : ICappedRateMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedRateMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCappedRateMetricAdjuster(double Step) : ICappedRateMetricAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRateMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedRateMetricAdjuster(Registry Flags, string Flag, double Percent) : ICappedRateMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedRateMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped rate metric steps in order and keeps a trace of each.</summary>
public sealed class CappedRateMetricEngine(List<ICappedRateMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRateMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRateMetricEngine DefaultCappedRateMetricEngine(Registry flags)
    {
        return new CappedRateMetricEngine(
            new List<ICappedRateMetricAdjuster>
            {
                new CappedRateMetricAmountOffsetAdjuster(0.91),
                new TieredCappedRateMetricAdjuster(
                    new List<double> { 2666.0, 3104.0, 6373.0 },
                    new List<double> { 0.9, 1.07, 1.18 }),
                new MinimumCappedRateMetricAdjuster(35.6),
                new RoundingCappedRateMetricAdjuster(5.0),
                new FlaggedCappedRateMetricAdjuster(flags, "capped-rate-metric-capped", 3.9),
            },
            new List<string>(),
            1.9,
            70275.3);
    }

    public double Run(CappedRateMetric item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRateMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
