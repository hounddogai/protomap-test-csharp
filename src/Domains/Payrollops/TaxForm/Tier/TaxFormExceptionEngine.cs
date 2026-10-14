namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form exception calculation.</summary>
public interface ITaxFormExceptionFactor
{
    string Describe();
    double Adjust(TaxFormException item, double valueValue);
}

public sealed record RoundingTaxFormExceptionAdjuster(double Step) : ITaxFormExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TaxFormExceptionQuantityOffsetAdjuster(double Weight) : ITaxFormExceptionFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(TaxFormException item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record PercentTaxFormExceptionAdjuster(double Percent) : ITaxFormExceptionFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTaxFormExceptionAdjuster(double Cap) : ITaxFormExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTaxFormExceptionAdjuster(double Minimum) : ITaxFormExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTaxFormExceptionAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormException item, double valueValue)
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

/// <summary>Runs the tax form exception steps in order and keeps a trace of each.</summary>
public sealed class TaxFormExceptionEngine(List<ITaxFormExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormExceptionEngine DefaultTaxFormExceptionEngine()
    {
        return new TaxFormExceptionEngine(
            new List<ITaxFormExceptionFactor>
            {
                new RoundingTaxFormExceptionAdjuster(1.0),
                new TaxFormExceptionQuantityOffsetAdjuster(0.95),
                new PercentTaxFormExceptionAdjuster(2.4),
                new CapTaxFormExceptionAdjuster(33549.0),
                new MinimumTaxFormExceptionAdjuster(12.4),
                new TieredTaxFormExceptionAdjuster(
                    new List<double> { 7443.0, 8546.0 },
                    new List<double> { 1.0, 0.85 }),
            },
            new List<string>(),
            2.7,
            17412.3);
    }

    public double Evaluate(TaxFormException item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
