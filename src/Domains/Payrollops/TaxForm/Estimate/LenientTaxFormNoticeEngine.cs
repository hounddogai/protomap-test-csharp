namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient tax form notice calculation.</summary>
public interface ILenientTaxFormNoticeStep
{
    string Describe();
    double Adjust(LenientTaxFormNotice item, double valueValue);
}

public sealed record LenientTaxFormNoticeDiscountOffsetAdjuster(double Weight) : ILenientTaxFormNoticeStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(LenientTaxFormNotice item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record PercentLenientTaxFormNoticeAdjuster(double Percent) : ILenientTaxFormNoticeStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientTaxFormNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientTaxFormNoticeAdjuster(double Step) : ILenientTaxFormNoticeStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientTaxFormNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient tax form notice steps in order and keeps a trace of each.</summary>
public sealed class LenientTaxFormNoticeEngine(List<ILenientTaxFormNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTaxFormNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTaxFormNoticeEngine DefaultLenientTaxFormNoticeEngine()
    {
        return new LenientTaxFormNoticeEngine(
            new List<ILenientTaxFormNoticeStep>
            {
                new LenientTaxFormNoticeDiscountOffsetAdjuster(1.91),
                new PercentLenientTaxFormNoticeAdjuster(-24.1),
                new RoundingLenientTaxFormNoticeAdjuster(1.0),
            },
            new List<string>(),
            4.6,
            38467.6);
    }

    public double Price(LenientTaxFormNotice item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTaxFormNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
