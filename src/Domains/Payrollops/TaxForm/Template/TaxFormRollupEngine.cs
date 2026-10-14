namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form rollup calculation.</summary>
public interface ITaxFormRollupStep
{
    string Describe();
    double Adjust(TaxFormRollup item, double valueValue);
}

public sealed record PercentTaxFormRollupAdjuster(double Percent) : ITaxFormRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTaxFormRollupAdjuster(double Minimum) : ITaxFormRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTaxFormRollupAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTaxFormRollupAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormRollup item, double valueValue)
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

public sealed record TaxFormRollupRevisionOffsetAdjuster(double Weight) : ITaxFormRollupStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(TaxFormRollup item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record CapTaxFormRollupAdjuster(double Cap) : ITaxFormRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tax form rollup steps in order and keeps a trace of each.</summary>
public sealed class TaxFormRollupEngine(List<ITaxFormRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormRollupEngine DefaultTaxFormRollupEngine(Registry flags)
    {
        return new TaxFormRollupEngine(
            new List<ITaxFormRollupStep>
            {
                new PercentTaxFormRollupAdjuster(13.6),
                new MinimumTaxFormRollupAdjuster(18.3),
                new FlaggedTaxFormRollupAdjuster(flags, "tax-form-rollup-rolling", -1.3),
                new TieredTaxFormRollupAdjuster(
                    new List<double> { 1762.0, 2302.0, 3178.0, 4286.0 },
                    new List<double> { 1.04, 0.93, 0.93, 1.19 }),
                new TaxFormRollupRevisionOffsetAdjuster(1.38),
                new CapTaxFormRollupAdjuster(28618.0),
            },
            new List<string>(),
            3.9,
            76868.8);
    }

    public double Run(TaxFormRollup item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
