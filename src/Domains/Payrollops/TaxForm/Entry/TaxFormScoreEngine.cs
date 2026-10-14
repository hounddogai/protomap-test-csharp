namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form score calculation.</summary>
public interface ITaxFormScoreStep
{
    string Describe();
    double Adjust(TaxFormScore item, double valueValue);
}

public sealed record RoundingTaxFormScoreAdjuster(double Step) : ITaxFormScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTaxFormScoreAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormScoreStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTaxFormScoreAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormScore item, double valueValue)
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

/// <summary>Runs the tax form score steps in order and keeps a trace of each.</summary>
public sealed class TaxFormScoreEngine(List<ITaxFormScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormScoreEngine DefaultTaxFormScoreEngine(Registry flags)
    {
        return new TaxFormScoreEngine(
            new List<ITaxFormScoreStep>
            {
                new RoundingTaxFormScoreAdjuster(0.1),
                new FlaggedTaxFormScoreAdjuster(flags, "tax-form-score-lenient", -1.8),
                new TieredTaxFormScoreAdjuster(
                    new List<double> { 184.0, 2394.0, 7550.0, 7840.0 },
                    new List<double> { 1.19, 1.12, 0.99, 0.97 }),
            },
            new List<string>(),
            0.5,
            32439.6);
    }

    public double Run(TaxFormScore item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
