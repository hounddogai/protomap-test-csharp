namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form segment calculation.</summary>
public interface ITaxFormSegmentFactor
{
    string Describe();
    double Adjust(TaxFormSegment item, double valueValue);
}

public sealed record PercentTaxFormSegmentAdjuster(double Percent) : ITaxFormSegmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTaxFormSegmentAdjuster(double Step) : ITaxFormSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTaxFormSegmentAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTaxFormSegmentAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormSegment item, double valueValue)
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

public sealed record MinimumTaxFormSegmentAdjuster(double Minimum) : ITaxFormSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tax form segment steps in order and keeps a trace of each.</summary>
public sealed class TaxFormSegmentEngine(List<ITaxFormSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormSegmentEngine DefaultTaxFormSegmentEngine(Registry flags)
    {
        return new TaxFormSegmentEngine(
            new List<ITaxFormSegmentFactor>
            {
                new PercentTaxFormSegmentAdjuster(2.1),
                new RoundingTaxFormSegmentAdjuster(0.05),
                new FlaggedTaxFormSegmentAdjuster(flags, "tax-form-segment-global", -6.5),
                new TieredTaxFormSegmentAdjuster(
                    new List<double> { 2909.0, 5160.0, 7525.0, 8139.0 },
                    new List<double> { 1.02, 0.89, 0.99, 1.03 }),
                new MinimumTaxFormSegmentAdjuster(46.5),
            },
            new List<string>(),
            4.5,
            53973.6);
    }

    public double Compute(TaxFormSegment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
