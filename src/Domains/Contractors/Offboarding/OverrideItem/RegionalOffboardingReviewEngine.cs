namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional offboarding review calculation.</summary>
public interface IRegionalOffboardingReviewStep
{
    string Describe();
    double Adjust(RegionalOffboardingReview item, double valueValue);
}

public sealed record MinimumRegionalOffboardingReviewAdjuster(double Minimum) : IRegionalOffboardingReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalOffboardingReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalOffboardingReviewAdjuster(double Cap) : IRegionalOffboardingReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalOffboardingReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentRegionalOffboardingReviewAdjuster(Dictionary<string, double> Factors) : IRegionalOffboardingReviewStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RegionalOffboardingReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalOffboardingReviewAdjuster(double Percent) : IRegionalOffboardingReviewStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalOffboardingReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalOffboardingReviewMarginOffsetAdjuster(double Weight) : IRegionalOffboardingReviewStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(RegionalOffboardingReview item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredRegionalOffboardingReviewAdjuster(List<double> Bounds, List<double> Rates) : IRegionalOffboardingReviewStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalOffboardingReview item, double valueValue)
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

/// <summary>Runs the regional offboarding review steps in order and keeps a trace of each.</summary>
public sealed class RegionalOffboardingReviewEngine(List<IRegionalOffboardingReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalOffboardingReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalOffboardingReviewEngine DefaultRegionalOffboardingReviewEngine()
    {
        return new RegionalOffboardingReviewEngine(
            new List<IRegionalOffboardingReviewStep>
            {
                new MinimumRegionalOffboardingReviewAdjuster(28.7),
                new CapRegionalOffboardingReviewAdjuster(47087.0),
                new SegmentRegionalOffboardingReviewAdjuster(new Dictionary<string, double> { ["retail"] = 0.7 }),
                new PercentRegionalOffboardingReviewAdjuster(22.4),
                new RegionalOffboardingReviewMarginOffsetAdjuster(0.63),
                new TieredRegionalOffboardingReviewAdjuster(
                    new List<double> { 602.0, 1185.0, 1468.0, 1789.0 },
                    new List<double> { 1.08, 0.86, 1.08, 0.89 }),
            },
            new List<string>(),
            0.4,
            80842.2);
    }

    public double Evaluate(RegionalOffboardingReview item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalOffboardingReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
