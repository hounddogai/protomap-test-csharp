namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending tax form threshold calculation.</summary>
public interface IPendingTaxFormThresholdFactor
{
    string Describe();
    double Adjust(PendingTaxFormThreshold item, double valueValue);
}

public sealed record RegionPendingTaxFormThresholdAdjuster(Dictionary<string, double> Factors) : IPendingTaxFormThresholdFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PendingTaxFormThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPendingTaxFormThresholdAdjuster(List<double> Bounds, List<double> Rates) : IPendingTaxFormThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingTaxFormThreshold item, double valueValue)
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

public sealed record PercentPendingTaxFormThresholdAdjuster(double Percent) : IPendingTaxFormThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingTaxFormThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingTaxFormThresholdAdjuster(Registry Flags, string Flag, double Percent) : IPendingTaxFormThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingTaxFormThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending tax form threshold steps in order and keeps a trace of each.</summary>
public sealed class PendingTaxFormThresholdEngine(List<IPendingTaxFormThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingTaxFormThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingTaxFormThresholdEngine DefaultPendingTaxFormThresholdEngine(Registry flags)
    {
        return new PendingTaxFormThresholdEngine(
            new List<IPendingTaxFormThresholdFactor>
            {
                new RegionPendingTaxFormThresholdAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.04, ["south"] = 0.74, ["central"] = 0.99 }),
                new TieredPendingTaxFormThresholdAdjuster(
                    new List<double> { 4878.0, 6959.0, 7802.0 },
                    new List<double> { 1.07, 0.88, 0.83 }),
                new PercentPendingTaxFormThresholdAdjuster(-19.3),
                new FlaggedPendingTaxFormThresholdAdjuster(flags, "pending-tax-form-threshold-batched", 9.6),
            },
            new List<string>(),
            1.1,
            65670.2);
    }

    public double Compute(PendingTaxFormThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingTaxFormThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
