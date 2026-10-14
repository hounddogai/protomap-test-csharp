namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form template calculation.</summary>
public interface ITaxFormTemplateFactor
{
    string Describe();
    double Adjust(TaxFormTemplate item, double valueValue);
}

public sealed record MinimumTaxFormTemplateAdjuster(double Minimum) : ITaxFormTemplateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelTaxFormTemplateAdjuster(Dictionary<string, double> Factors) : ITaxFormTemplateFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TaxFormTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTaxFormTemplateAdjuster(double Step) : ITaxFormTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTaxFormTemplateAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormTemplate item, double valueValue)
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

public sealed record FlaggedTaxFormTemplateAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTaxFormTemplateAdjuster(double Percent) : ITaxFormTemplateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form template steps in order and keeps a trace of each.</summary>
public sealed class TaxFormTemplateEngine(List<ITaxFormTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormTemplateEngine DefaultTaxFormTemplateEngine(Registry flags)
    {
        return new TaxFormTemplateEngine(
            new List<ITaxFormTemplateFactor>
            {
                new MinimumTaxFormTemplateAdjuster(46.6),
                new LabelTaxFormTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.38, ["retail"] = 1.07 }),
                new RoundingTaxFormTemplateAdjuster(0.25),
                new TieredTaxFormTemplateAdjuster(
                    new List<double> { 426.0, 3294.0, 3504.0, 9584.0 },
                    new List<double> { 1.11, 1.02, 0.91, 1.11 }),
                new FlaggedTaxFormTemplateAdjuster(flags, "tax-form-template-strict", 0.3),
                new PercentTaxFormTemplateAdjuster(-14.6),
            },
            new List<string>(),
            1.1,
            53693.4);
    }

    public double Run(TaxFormTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
