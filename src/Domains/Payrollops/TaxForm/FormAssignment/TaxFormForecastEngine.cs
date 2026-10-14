namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form forecast calculation.</summary>
public interface ITaxFormForecastStep
{
    string Describe();
    double Adjust(TaxFormForecast item, double valueValue);
}

public sealed record TaxFormForecastEndsAtOffsetAdjuster(double Weight) : ITaxFormForecastStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(TaxFormForecast item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record RoundingTaxFormForecastAdjuster(double Step) : ITaxFormForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTaxFormForecastAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormForecastStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form forecast steps in order and keeps a trace of each.</summary>
public sealed class TaxFormForecastEngine(List<ITaxFormForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormForecastEngine DefaultTaxFormForecastEngine(Registry flags)
    {
        return new TaxFormForecastEngine(
            new List<ITaxFormForecastStep>
            {
                new TaxFormForecastEndsAtOffsetAdjuster(0.08),
                new RoundingTaxFormForecastAdjuster(0.1),
                new FlaggedTaxFormForecastAdjuster(flags, "tax-form-forecast-weighted", 3.4),
            },
            new List<string>(),
            2.6,
            84449.0);
    }

    public double Run(TaxFormForecast item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
