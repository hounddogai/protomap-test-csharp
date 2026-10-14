namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form cycle calculation.</summary>
public interface ITaxFormCycleFactor
{
    string Describe();
    double Adjust(TaxFormCycle item, double valueValue);
}

public sealed record FlaggedTaxFormCycleAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TaxFormCycleSequenceOffsetAdjuster(double Weight) : ITaxFormCycleFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(TaxFormCycle item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingTaxFormCycleAdjuster(double Step) : ITaxFormCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tax form cycle steps in order and keeps a trace of each.</summary>
public sealed class TaxFormCycleEngine(List<ITaxFormCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormCycleEngine DefaultTaxFormCycleEngine(Registry flags)
    {
        return new TaxFormCycleEngine(
            new List<ITaxFormCycleFactor>
            {
                new FlaggedTaxFormCycleAdjuster(flags, "tax-form-cycle-pending", 0.2),
                new TaxFormCycleSequenceOffsetAdjuster(0.55),
                new RoundingTaxFormCycleAdjuster(5.0),
            },
            new List<string>(),
            3.6,
            53970.4);
    }

    public double Run(TaxFormCycle item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
