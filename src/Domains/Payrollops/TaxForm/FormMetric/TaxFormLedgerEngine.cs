namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form ledger calculation.</summary>
public interface ITaxFormLedgerFactor
{
    string Describe();
    double Adjust(TaxFormLedger item, double valueValue);
}

public sealed record NameTaxFormLedgerAdjuster(Dictionary<string, double> Factors) : ITaxFormLedgerFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TaxFormLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTaxFormLedgerAdjuster(double Step) : ITaxFormLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TaxFormLedgerUpdatedAtOffsetAdjuster(double Weight) : ITaxFormLedgerFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(TaxFormLedger item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record MinimumTaxFormLedgerAdjuster(double Minimum) : ITaxFormLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTaxFormLedgerAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTaxFormLedgerAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormLedger item, double valueValue)
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

/// <summary>Runs the tax form ledger steps in order and keeps a trace of each.</summary>
public sealed class TaxFormLedgerEngine(List<ITaxFormLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormLedgerEngine DefaultTaxFormLedgerEngine(Registry flags)
    {
        return new TaxFormLedgerEngine(
            new List<ITaxFormLedgerFactor>
            {
                new NameTaxFormLedgerAdjuster(new Dictionary<string, double> { ["north"] = 1.37 }),
                new RoundingTaxFormLedgerAdjuster(0.5),
                new TaxFormLedgerUpdatedAtOffsetAdjuster(0.97),
                new MinimumTaxFormLedgerAdjuster(22.2),
                new FlaggedTaxFormLedgerAdjuster(flags, "tax-form-ledger-regional", 6.0),
                new TieredTaxFormLedgerAdjuster(new List<double> { 92.0, 1506.0 }, new List<double> { 1.08, 1.03 }),
            },
            new List<string>(),
            3.5,
            26405.6);
    }

    public double Price(TaxFormLedger item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
