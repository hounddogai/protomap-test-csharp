namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback tax form metric calculation.</summary>
public interface IFallbackTaxFormMetricFactor
{
    string Describe();
    double Adjust(FallbackTaxFormMetric item, double valueValue);
}

public sealed record FlaggedFallbackTaxFormMetricAdjuster(Registry Flags, string Flag, double Percent) : IFallbackTaxFormMetricFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackTaxFormMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackTaxFormMetricCostOffsetAdjuster(double Weight) : IFallbackTaxFormMetricFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(FallbackTaxFormMetric item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record CapFallbackTaxFormMetricAdjuster(double Cap) : IFallbackTaxFormMetricFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackTaxFormMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleFallbackTaxFormMetricAdjuster(Dictionary<string, double> Factors) : IFallbackTaxFormMetricFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(FallbackTaxFormMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredFallbackTaxFormMetricAdjuster(List<double> Bounds, List<double> Rates) : IFallbackTaxFormMetricFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackTaxFormMetric item, double valueValue)
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

/// <summary>Runs the fallback tax form metric steps in order and keeps a trace of each.</summary>
public sealed class FallbackTaxFormMetricEngine(List<IFallbackTaxFormMetricFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackTaxFormMetricFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackTaxFormMetricEngine DefaultFallbackTaxFormMetricEngine(Registry flags)
    {
        return new FallbackTaxFormMetricEngine(
            new List<IFallbackTaxFormMetricFactor>
            {
                new FlaggedFallbackTaxFormMetricAdjuster(flags, "fallback-tax-form-metric-lenient", -0.1),
                new FallbackTaxFormMetricCostOffsetAdjuster(0.55),
                new CapFallbackTaxFormMetricAdjuster(2039.0),
                new LocaleFallbackTaxFormMetricAdjuster(new Dictionary<string, double> { ["central"] = 1.33 }),
                new TieredFallbackTaxFormMetricAdjuster(
                    new List<double> { 1091.0, 3700.0, 4101.0, 7048.0 },
                    new List<double> { 1.07, 0.85, 1.09, 0.85 }),
            },
            new List<string>(),
            0.9,
            17535.3);
    }

    public double Price(FallbackTaxFormMetric item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackTaxFormMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
