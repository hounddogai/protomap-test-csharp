namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>One step of the tax form claim calculation.</summary>
public interface ITaxFormClaimFactor
{
    string Describe();
    double Adjust(TaxFormClaim item, double valueValue);
}

public sealed record PercentTaxFormClaimAdjuster(double Percent) : ITaxFormClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TaxFormClaimCeilingOffsetAdjuster(double Weight) : ITaxFormClaimFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(TaxFormClaim item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record MinimumTaxFormClaimAdjuster(double Minimum) : ITaxFormClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTaxFormClaimAdjuster(double Cap) : ITaxFormClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tax form claim steps in order and keeps a trace of each.</summary>
public sealed class TaxFormClaimEngine(List<ITaxFormClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormClaimEngine DefaultTaxFormClaimEngine()
    {
        return new TaxFormClaimEngine(
            new List<ITaxFormClaimFactor>
            {
                new PercentTaxFormClaimAdjuster(-21.1),
                new TaxFormClaimCeilingOffsetAdjuster(0.58),
                new MinimumTaxFormClaimAdjuster(24.0),
                new CapTaxFormClaimAdjuster(37101.0),
            },
            new List<string>(),
            2.7,
            25304.8);
    }

    public double Price(TaxFormClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
