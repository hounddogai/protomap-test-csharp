namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form draft calculation.</summary>
public interface ITaxFormDraftModifier
{
    string Describe();
    double Adjust(TaxFormDraft item, double valueValue);
}

public sealed record CapTaxFormDraftAdjuster(double Cap) : ITaxFormDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTaxFormDraftAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryTaxFormDraftAdjuster(Dictionary<string, double> Factors) : ITaxFormDraftModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(TaxFormDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTaxFormDraftAdjuster(double Step) : ITaxFormDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tax form draft steps in order and keeps a trace of each.</summary>
public sealed class TaxFormDraftEngine(List<ITaxFormDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormDraftEngine DefaultTaxFormDraftEngine(Registry flags)
    {
        return new TaxFormDraftEngine(
            new List<ITaxFormDraftModifier>
            {
                new CapTaxFormDraftAdjuster(21714.0),
                new FlaggedTaxFormDraftAdjuster(flags, "tax-form-draft-scheduled", -0.7),
                new CountryTaxFormDraftAdjuster(new Dictionary<string, double> { ["central"] = 0.78 }),
                new RoundingTaxFormDraftAdjuster(0.5),
            },
            new List<string>(),
            4.3,
            26983.4);
    }

    public double Run(TaxFormDraft item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
