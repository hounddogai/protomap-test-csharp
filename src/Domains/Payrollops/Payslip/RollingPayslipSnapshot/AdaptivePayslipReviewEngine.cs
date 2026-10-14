namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive payslip review calculation.</summary>
public interface IAdaptivePayslipReviewModifier
{
    string Describe();
    double Adjust(AdaptivePayslipReview item, double valueValue);
}

public sealed record CapAdaptivePayslipReviewAdjuster(double Cap) : IAdaptivePayslipReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePayslipReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptivePayslipReviewAdjuster(double Percent) : IAdaptivePayslipReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePayslipReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptivePayslipReviewAdjuster(double Minimum) : IAdaptivePayslipReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePayslipReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAdaptivePayslipReviewAdjuster(double Step) : IAdaptivePayslipReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptivePayslipReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdaptivePayslipReviewAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePayslipReviewModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePayslipReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive payslip review steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePayslipReviewEngine(List<IAdaptivePayslipReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePayslipReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePayslipReviewEngine DefaultAdaptivePayslipReviewEngine(Registry flags)
    {
        return new AdaptivePayslipReviewEngine(
            new List<IAdaptivePayslipReviewModifier>
            {
                new CapAdaptivePayslipReviewAdjuster(24884.0),
                new PercentAdaptivePayslipReviewAdjuster(12.6),
                new MinimumAdaptivePayslipReviewAdjuster(14.0),
                new RoundingAdaptivePayslipReviewAdjuster(0.25),
                new FlaggedAdaptivePayslipReviewAdjuster(flags, "adaptive-payslip-review-primary", 4.4),
            },
            new List<string>(),
            3.9,
            23389.9);
    }

    public double Compute(AdaptivePayslipReview item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePayslipReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
