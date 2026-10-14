namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form tier calculation.</summary>
public interface ITaxFormTierModifier
{
    string Describe();
    double Adjust(TaxFormTier item, double valueValue);
}

public sealed record PercentTaxFormTierAdjuster(double Percent) : ITaxFormTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTaxFormTierAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormTier item, double valueValue)
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

public sealed record RoundingTaxFormTierAdjuster(double Step) : ITaxFormTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentTaxFormTierAdjuster(Dictionary<string, double> Factors) : ITaxFormTierModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TaxFormTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TaxFormTierBatchSizeOffsetAdjuster(double Weight) : ITaxFormTierModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(TaxFormTier item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

/// <summary>Runs the tax form tier steps in order and keeps a trace of each.</summary>
public sealed class TaxFormTierEngine(List<ITaxFormTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormTierEngine DefaultTaxFormTierEngine()
    {
        return new TaxFormTierEngine(
            new List<ITaxFormTierModifier>
            {
                new PercentTaxFormTierAdjuster(15.5),
                new TieredTaxFormTierAdjuster(new List<double> { 3087.0, 6272.0 }, new List<double> { 0.96, 0.92 }),
                new RoundingTaxFormTierAdjuster(5.0),
                new SegmentTaxFormTierAdjuster(new Dictionary<string, double> { ["retail"] = 0.91, ["west"] = 1.1 }),
                new TaxFormTierBatchSizeOffsetAdjuster(1.25),
            },
            new List<string>(),
            4.4,
            62026.1);
    }

    public double Price(TaxFormTier item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
