namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tax form mapping calculation.</summary>
public interface ITaxFormMappingAdjuster
{
    string Describe();
    double Adjust(TaxFormMapping item, double valueValue);
}

public sealed record TieredTaxFormMappingAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormMapping item, double valueValue)
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

public sealed record PercentTaxFormMappingAdjuster(double Percent) : ITaxFormMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTaxFormMappingAdjuster(double Minimum) : ITaxFormMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TaxFormMappingDiscountOffsetAdjuster(double Weight) : ITaxFormMappingAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(TaxFormMapping item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record FlaggedTaxFormMappingAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionTaxFormMappingAdjuster(Dictionary<string, double> Factors) : ITaxFormMappingAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TaxFormMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tax form mapping steps in order and keeps a trace of each.</summary>
public sealed class TaxFormMappingEngine(List<ITaxFormMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormMappingEngine DefaultTaxFormMappingEngine(Registry flags)
    {
        return new TaxFormMappingEngine(
            new List<ITaxFormMappingAdjuster>
            {
                new TieredTaxFormMappingAdjuster(
                    new List<double> { 235.0, 4239.0, 5171.0, 5831.0 },
                    new List<double> { 1.16, 1.18, 0.91, 0.82 }),
                new PercentTaxFormMappingAdjuster(-9.1),
                new MinimumTaxFormMappingAdjuster(2.3),
                new TaxFormMappingDiscountOffsetAdjuster(0.82),
                new FlaggedTaxFormMappingAdjuster(flags, "tax-form-mapping-seasonal", 3.5),
                new RegionTaxFormMappingAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.98, ["online"] = 1.34, ["south"] = 0.89 }),
            },
            new List<string>(),
            0.3,
            77539.4);
    }

    public double Price(TaxFormMapping item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
