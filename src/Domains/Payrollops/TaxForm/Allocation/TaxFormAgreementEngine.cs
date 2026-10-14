namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form agreement calculation.</summary>
public interface ITaxFormAgreementModifier
{
    string Describe();
    double Adjust(TaxFormAgreement item, double valueValue);
}

public sealed record PercentTaxFormAgreementAdjuster(double Percent) : ITaxFormAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTaxFormAgreementAdjuster(double Step) : ITaxFormAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceTaxFormAgreementAdjuster(Dictionary<string, double> Factors) : ITaxFormAgreementModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TaxFormAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tax form agreement steps in order and keeps a trace of each.</summary>
public sealed class TaxFormAgreementEngine(List<ITaxFormAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormAgreementEngine DefaultTaxFormAgreementEngine()
    {
        return new TaxFormAgreementEngine(
            new List<ITaxFormAgreementModifier>
            {
                new PercentTaxFormAgreementAdjuster(-21.9),
                new RoundingTaxFormAgreementAdjuster(0.25),
                new SourceTaxFormAgreementAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.27, ["north"] = 1.0 }),
            },
            new List<string>(),
            1.2,
            20108.5);
    }

    public double Compute(TaxFormAgreement item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
