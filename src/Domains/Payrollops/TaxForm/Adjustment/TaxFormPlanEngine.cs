namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form plan calculation.</summary>
public interface ITaxFormPlanAdjuster
{
    string Describe();
    double Adjust(TaxFormPlan item, double valueValue);
}

public sealed record RoundingTaxFormPlanAdjuster(double Step) : ITaxFormPlanAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTaxFormPlanAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTaxFormPlanAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormPlanAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormPlan item, double valueValue)
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

public sealed record MinimumTaxFormPlanAdjuster(double Minimum) : ITaxFormPlanAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tax form plan steps in order and keeps a trace of each.</summary>
public sealed class TaxFormPlanEngine(List<ITaxFormPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormPlanEngine DefaultTaxFormPlanEngine(Registry flags)
    {
        return new TaxFormPlanEngine(
            new List<ITaxFormPlanAdjuster>
            {
                new RoundingTaxFormPlanAdjuster(0.25),
                new FlaggedTaxFormPlanAdjuster(flags, "tax-form-plan-weighted", 7.9),
                new TieredTaxFormPlanAdjuster(new List<double> { 7190.0, 8278.0 }, new List<double> { 0.85, 0.82 }),
                new MinimumTaxFormPlanAdjuster(34.2),
            },
            new List<string>(),
            1.0,
            13700.1);
    }

    public double Evaluate(TaxFormPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
