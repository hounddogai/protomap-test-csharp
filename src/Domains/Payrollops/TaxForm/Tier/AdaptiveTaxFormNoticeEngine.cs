namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive tax form notice calculation.</summary>
public interface IAdaptiveTaxFormNoticeModifier
{
    string Describe();
    double Adjust(AdaptiveTaxFormNotice item, double valueValue);
}

public sealed record TieredAdaptiveTaxFormNoticeAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveTaxFormNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveTaxFormNotice item, double valueValue)
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

public sealed record RoundingAdaptiveTaxFormNoticeAdjuster(double Step) : IAdaptiveTaxFormNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTaxFormNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptiveTaxFormNoticeAdjuster(double Minimum) : IAdaptiveTaxFormNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveTaxFormNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive tax form notice steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTaxFormNoticeEngine(List<IAdaptiveTaxFormNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTaxFormNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTaxFormNoticeEngine DefaultAdaptiveTaxFormNoticeEngine()
    {
        return new AdaptiveTaxFormNoticeEngine(
            new List<IAdaptiveTaxFormNoticeModifier>
            {
                new TieredAdaptiveTaxFormNoticeAdjuster(
                    new List<double> { 3741.0, 3792.0, 7233.0, 8885.0 },
                    new List<double> { 0.9, 0.82, 0.9, 1.0 }),
                new RoundingAdaptiveTaxFormNoticeAdjuster(5.0),
                new MinimumAdaptiveTaxFormNoticeAdjuster(25.7),
            },
            new List<string>(),
            1.2,
            51531.3);
    }

    public double Compute(AdaptiveTaxFormNotice item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTaxFormNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
