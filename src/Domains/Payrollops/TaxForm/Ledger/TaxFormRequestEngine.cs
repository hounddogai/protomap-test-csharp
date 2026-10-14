namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form request calculation.</summary>
public interface ITaxFormRequestFactor
{
    string Describe();
    double Adjust(TaxFormRequest item, double valueValue);
}

public sealed record TieredTaxFormRequestAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormRequest item, double valueValue)
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

public sealed record RegionTaxFormRequestAdjuster(Dictionary<string, double> Factors) : ITaxFormRequestFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TaxFormRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTaxFormRequestAdjuster(double Minimum) : ITaxFormRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tax form request steps in order and keeps a trace of each.</summary>
public sealed class TaxFormRequestEngine(List<ITaxFormRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormRequestEngine DefaultTaxFormRequestEngine()
    {
        return new TaxFormRequestEngine(
            new List<ITaxFormRequestFactor>
            {
                new TieredTaxFormRequestAdjuster(
                    new List<double> { 1272.0, 1402.0, 1485.0, 3264.0 },
                    new List<double> { 1.04, 0.94, 0.97, 0.89 }),
                new RegionTaxFormRequestAdjuster(new Dictionary<string, double> { ["online"] = 1.17 }),
                new MinimumTaxFormRequestAdjuster(0.1),
            },
            new List<string>(),
            1.4,
            93202.0);
    }

    public double Compute(TaxFormRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
