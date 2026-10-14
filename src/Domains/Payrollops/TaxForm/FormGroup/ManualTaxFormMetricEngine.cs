namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual tax form metric calculation.</summary>
public interface IManualTaxFormMetricStep
{
    string Describe();
    double Adjust(ManualTaxFormMetric item, double valueValue);
}

public sealed record PercentManualTaxFormMetricAdjuster(double Percent) : IManualTaxFormMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualTaxFormMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryManualTaxFormMetricAdjuster(Dictionary<string, double> Factors) : IManualTaxFormMetricStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ManualTaxFormMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualTaxFormMetricAdjuster(List<double> Bounds, List<double> Rates) : IManualTaxFormMetricStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualTaxFormMetric item, double valueValue)
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

/// <summary>Runs the manual tax form metric steps in order and keeps a trace of each.</summary>
public sealed class ManualTaxFormMetricEngine(List<IManualTaxFormMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualTaxFormMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualTaxFormMetricEngine DefaultManualTaxFormMetricEngine()
    {
        return new ManualTaxFormMetricEngine(
            new List<IManualTaxFormMetricStep>
            {
                new PercentManualTaxFormMetricAdjuster(-6.4),
                new CategoryManualTaxFormMetricAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.05, ["west"] = 0.86 }),
                new TieredManualTaxFormMetricAdjuster(
                    new List<double> { 7148.0, 7197.0, 7821.0, 9461.0 },
                    new List<double> { 1.17, 0.83, 1.06, 0.82 }),
            },
            new List<string>(),
            1.0,
            90956.7);
    }

    public double Run(ManualTaxFormMetric item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualTaxFormMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
