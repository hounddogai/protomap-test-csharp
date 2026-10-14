namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tax form estimate calculation.</summary>
public interface ITaxFormEstimateStep
{
    string Describe();
    double Adjust(TaxFormEstimate item, double valueValue);
}

public sealed record TieredTaxFormEstimateAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormEstimate item, double valueValue)
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

public sealed record MinimumTaxFormEstimateAdjuster(double Minimum) : ITaxFormEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleTaxFormEstimateAdjuster(Dictionary<string, double> Factors) : ITaxFormEstimateStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TaxFormEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapTaxFormEstimateAdjuster(double Cap) : ITaxFormEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTaxFormEstimateAdjuster(double Percent) : ITaxFormEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTaxFormEstimateAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form estimate steps in order and keeps a trace of each.</summary>
public sealed class TaxFormEstimateEngine(List<ITaxFormEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormEstimateEngine DefaultTaxFormEstimateEngine(Registry flags)
    {
        return new TaxFormEstimateEngine(
            new List<ITaxFormEstimateStep>
            {
                new TieredTaxFormEstimateAdjuster(
                    new List<double> { 927.0, 4490.0, 7203.0, 9992.0 },
                    new List<double> { 0.89, 1.07, 0.87, 0.85 }),
                new MinimumTaxFormEstimateAdjuster(34.6),
                new TitleTaxFormEstimateAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.78, ["west"] = 0.9, ["retail"] = 1.01 }),
                new CapTaxFormEstimateAdjuster(12115.0),
                new PercentTaxFormEstimateAdjuster(-12.3),
                new FlaggedTaxFormEstimateAdjuster(flags, "tax-form-estimate-tiered", -1.8),
            },
            new List<string>(),
            3.9,
            91592.7);
    }

    public double Run(TaxFormEstimate item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
