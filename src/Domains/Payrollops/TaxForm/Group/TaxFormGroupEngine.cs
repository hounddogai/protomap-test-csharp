namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form group calculation.</summary>
public interface ITaxFormGroupModifier
{
    string Describe();
    double Adjust(TaxFormGroup item, double valueValue);
}

public sealed record PercentTaxFormGroupAdjuster(double Percent) : ITaxFormGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TaxFormGroupSequenceOffsetAdjuster(double Weight) : ITaxFormGroupModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(TaxFormGroup item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingTaxFormGroupAdjuster(double Step) : ITaxFormGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTaxFormGroupAdjuster(double Cap) : ITaxFormGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tax form group steps in order and keeps a trace of each.</summary>
public sealed class TaxFormGroupEngine(List<ITaxFormGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormGroupEngine DefaultTaxFormGroupEngine()
    {
        return new TaxFormGroupEngine(
            new List<ITaxFormGroupModifier>
            {
                new PercentTaxFormGroupAdjuster(-15.5),
                new TaxFormGroupSequenceOffsetAdjuster(1.72),
                new RoundingTaxFormGroupAdjuster(1.0),
                new CapTaxFormGroupAdjuster(13267.0),
            },
            new List<string>(),
            1.3,
            6486.3);
    }

    public double Compute(TaxFormGroup item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
