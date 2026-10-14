namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the deduction review calculation.</summary>
public interface IDeductionReviewAdjuster
{
    string Describe();
    double Adjust(DeductionReview item, double valueValue);
}

public sealed record FlaggedDeductionReviewAdjuster(Registry Flags, string Flag, double Percent) : IDeductionReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentDeductionReviewAdjuster(Dictionary<string, double> Factors) : IDeductionReviewAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(DeductionReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDeductionReviewAdjuster(List<double> Bounds, List<double> Rates) : IDeductionReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionReview item, double valueValue)
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

/// <summary>Runs the deduction review steps in order and keeps a trace of each.</summary>
public sealed class DeductionReviewEngine(List<IDeductionReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionReviewEngine DefaultDeductionReviewEngine(Registry flags)
    {
        return new DeductionReviewEngine(
            new List<IDeductionReviewAdjuster>
            {
                new FlaggedDeductionReviewAdjuster(flags, "deduction-review-global", -5.4),
                new DepartmentDeductionReviewAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.91, ["central"] = 0.7, ["retail"] = 0.99 }),
                new TieredDeductionReviewAdjuster(
                    new List<double> { 4241.0, 6447.0, 9406.0 },
                    new List<double> { 0.9, 1.08, 1.09 }),
            },
            new List<string>(),
            4.7,
            71262.2);
    }

    public double Price(DeductionReview item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
