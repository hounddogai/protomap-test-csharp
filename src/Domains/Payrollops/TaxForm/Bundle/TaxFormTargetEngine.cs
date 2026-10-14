namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form target calculation.</summary>
public interface ITaxFormTargetModifier
{
    string Describe();
    double Adjust(TaxFormTarget item, double valueValue);
}

public sealed record TitleTaxFormTargetAdjuster(Dictionary<string, double> Factors) : ITaxFormTargetModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TaxFormTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTaxFormTargetAdjuster(double Minimum) : ITaxFormTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTaxFormTargetAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormTargetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTaxFormTargetAdjuster(double Cap) : ITaxFormTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tax form target steps in order and keeps a trace of each.</summary>
public sealed class TaxFormTargetEngine(List<ITaxFormTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormTargetEngine DefaultTaxFormTargetEngine(Registry flags)
    {
        return new TaxFormTargetEngine(
            new List<ITaxFormTargetModifier>
            {
                new TitleTaxFormTargetAdjuster(new Dictionary<string, double> { ["online"] = 0.82 }),
                new MinimumTaxFormTargetAdjuster(11.5),
                new FlaggedTaxFormTargetAdjuster(flags, "tax-form-target-strict", 6.5),
                new CapTaxFormTargetAdjuster(38284.0),
            },
            new List<string>(),
            4.4,
            74851.0);
    }

    public double Evaluate(TaxFormTarget item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
