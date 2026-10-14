namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual tax form audit calculation.</summary>
public interface IManualTaxFormAuditStep
{
    string Describe();
    double Adjust(ManualTaxFormAudit item, double valueValue);
}

public sealed record PercentManualTaxFormAuditAdjuster(double Percent) : IManualTaxFormAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualTaxFormAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelManualTaxFormAuditAdjuster(Dictionary<string, double> Factors) : IManualTaxFormAuditStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ManualTaxFormAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualTaxFormAuditAdjuster(List<double> Bounds, List<double> Rates) : IManualTaxFormAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualTaxFormAudit item, double valueValue)
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

public sealed record ManualTaxFormAuditCapacityOffsetAdjuster(double Weight) : IManualTaxFormAuditStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(ManualTaxFormAudit item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the manual tax form audit steps in order and keeps a trace of each.</summary>
public sealed class ManualTaxFormAuditEngine(List<IManualTaxFormAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualTaxFormAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualTaxFormAuditEngine DefaultManualTaxFormAuditEngine()
    {
        return new ManualTaxFormAuditEngine(
            new List<IManualTaxFormAuditStep>
            {
                new PercentManualTaxFormAuditAdjuster(20.6),
                new ChannelManualTaxFormAuditAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.05, ["retail"] = 1.29 }),
                new TieredManualTaxFormAuditAdjuster(
                    new List<double> { 5374.0, 7617.0 },
                    new List<double> { 0.97, 0.94 }),
                new ManualTaxFormAuditCapacityOffsetAdjuster(1.99),
            },
            new List<string>(),
            3.6,
            70081.3);
    }

    public double Compute(ManualTaxFormAudit item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualTaxFormAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
