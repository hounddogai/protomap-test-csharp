namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form allocation calculation.</summary>
public interface ITaxFormAllocationModifier
{
    string Describe();
    double Adjust(TaxFormAllocation item, double valueValue);
}

public sealed record CapTaxFormAllocationAdjuster(double Cap) : ITaxFormAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTaxFormAllocationAdjuster(double Step) : ITaxFormAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelTaxFormAllocationAdjuster(Dictionary<string, double> Factors) : ITaxFormAllocationModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TaxFormAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTaxFormAllocationAdjuster(double Percent) : ITaxFormAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form allocation steps in order and keeps a trace of each.</summary>
public sealed class TaxFormAllocationEngine(List<ITaxFormAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormAllocationEngine DefaultTaxFormAllocationEngine()
    {
        return new TaxFormAllocationEngine(
            new List<ITaxFormAllocationModifier>
            {
                new CapTaxFormAllocationAdjuster(45627.0),
                new RoundingTaxFormAllocationAdjuster(0.25),
                new LabelTaxFormAllocationAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.33, ["east"] = 1.2 }),
                new PercentTaxFormAllocationAdjuster(-19.0),
            },
            new List<string>(),
            0.4,
            36492.0);
    }

    public double Price(TaxFormAllocation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
