namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tax form slot calculation.</summary>
public interface ITaxFormSlotModifier
{
    string Describe();
    double Adjust(TaxFormSlot item, double valueValue);
}

public sealed record MinimumTaxFormSlotAdjuster(double Minimum) : ITaxFormSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTaxFormSlotAdjuster(double Cap) : ITaxFormSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTaxFormSlotAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form slot steps in order and keeps a trace of each.</summary>
public sealed class TaxFormSlotEngine(List<ITaxFormSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormSlotEngine DefaultTaxFormSlotEngine(Registry flags)
    {
        return new TaxFormSlotEngine(
            new List<ITaxFormSlotModifier>
            {
                new MinimumTaxFormSlotAdjuster(44.8),
                new CapTaxFormSlotAdjuster(37910.0),
                new FlaggedTaxFormSlotAdjuster(flags, "tax-form-slot-lenient", -6.3),
            },
            new List<string>(),
            4.1,
            68469.3);
    }

    public double Compute(TaxFormSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
