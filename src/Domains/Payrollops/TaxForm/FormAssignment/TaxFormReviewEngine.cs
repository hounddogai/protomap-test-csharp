namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form review calculation.</summary>
public interface ITaxFormReviewAdjuster
{
    string Describe();
    double Adjust(TaxFormReview item, double valueValue);
}

public sealed record TieredTaxFormReviewAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormReview item, double valueValue)
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

public sealed record MinimumTaxFormReviewAdjuster(double Minimum) : ITaxFormReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTaxFormReviewAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTaxFormReviewAdjuster(double Cap) : ITaxFormReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTaxFormReviewAdjuster(double Step) : ITaxFormReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SourceTaxFormReviewAdjuster(Dictionary<string, double> Factors) : ITaxFormReviewAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TaxFormReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tax form review steps in order and keeps a trace of each.</summary>
public sealed class TaxFormReviewEngine(List<ITaxFormReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormReviewEngine DefaultTaxFormReviewEngine(Registry flags)
    {
        return new TaxFormReviewEngine(
            new List<ITaxFormReviewAdjuster>
            {
                new TieredTaxFormReviewAdjuster(new List<double> { 377.0, 9692.0 }, new List<double> { 1.11, 1.04 }),
                new MinimumTaxFormReviewAdjuster(37.1),
                new FlaggedTaxFormReviewAdjuster(flags, "tax-form-review-strict", -7.3),
                new CapTaxFormReviewAdjuster(40681.0),
                new RoundingTaxFormReviewAdjuster(1.0),
                new SourceTaxFormReviewAdjuster(new Dictionary<string, double> { ["north"] = 1.16 }),
            },
            new List<string>(),
            4.4,
            65421.5);
    }

    public double Compute(TaxFormReview item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
