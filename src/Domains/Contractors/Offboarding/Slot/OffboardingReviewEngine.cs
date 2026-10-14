namespace Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>One step of the offboarding review calculation.</summary>
public interface IOffboardingReviewFactor
{
    string Describe();
    double Adjust(OffboardingReview item, double valueValue);
}

public sealed record PercentOffboardingReviewAdjuster(double Percent) : IOffboardingReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOffboardingReviewAdjuster(double Cap) : IOffboardingReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryOffboardingReviewAdjuster(Dictionary<string, double> Factors) : IOffboardingReviewFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(OffboardingReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumOffboardingReviewAdjuster(double Minimum) : IOffboardingReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the offboarding review steps in order and keeps a trace of each.</summary>
public sealed class OffboardingReviewEngine(List<IOffboardingReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingReviewEngine DefaultOffboardingReviewEngine()
    {
        return new OffboardingReviewEngine(
            new List<IOffboardingReviewFactor>
            {
                new PercentOffboardingReviewAdjuster(-14.7),
                new CapOffboardingReviewAdjuster(9745.0),
                new CountryOffboardingReviewAdjuster(new Dictionary<string, double> { ["online"] = 0.77 }),
                new MinimumOffboardingReviewAdjuster(9.6),
            },
            new List<string>(),
            4.0,
            87388.4);
    }

    public double Run(OffboardingReview item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
