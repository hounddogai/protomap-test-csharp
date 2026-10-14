namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form metric calculation.</summary>
public interface ITaxFormMetricModifier
{
    string Describe();
    double Adjust(TaxFormMetric item, double valueValue);
}

public sealed record MinimumTaxFormMetricAdjuster(double Minimum) : ITaxFormMetricModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTaxFormMetricAdjuster(double Cap) : ITaxFormMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTaxFormMetricAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormMetricModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormMetric item, double valueValue)
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

public sealed record FlaggedTaxFormMetricAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormMetricModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form metric steps in order and keeps a trace of each.</summary>
public sealed class TaxFormMetricEngine(List<ITaxFormMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormMetricEngine DefaultTaxFormMetricEngine(Registry flags)
    {
        return new TaxFormMetricEngine(
            new List<ITaxFormMetricModifier>
            {
                new MinimumTaxFormMetricAdjuster(2.7),
                new CapTaxFormMetricAdjuster(24110.0),
                new TieredTaxFormMetricAdjuster(
                    new List<double> { 4331.0, 5733.0, 9430.0 },
                    new List<double> { 0.92, 1.13, 1.04 }),
                new FlaggedTaxFormMetricAdjuster(flags, "tax-form-metric-lenient", 6.1),
            },
            new List<string>(),
            4.4,
            50586.0);
    }

    public double Run(TaxFormMetric item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
