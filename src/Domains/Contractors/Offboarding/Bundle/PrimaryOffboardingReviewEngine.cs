namespace Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>One step of the primary offboarding review calculation.</summary>
public interface IPrimaryOffboardingReviewModifier
{
    string Describe();
    double Adjust(PrimaryOffboardingReview item, double valueValue);
}

public sealed record PrimaryOffboardingReviewMinimumOffsetAdjuster(double Weight) : IPrimaryOffboardingReviewModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PrimaryOffboardingReview item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record PercentPrimaryOffboardingReviewAdjuster(double Percent) : IPrimaryOffboardingReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryOffboardingReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryOffboardingReviewAdjuster(double Cap) : IPrimaryOffboardingReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryOffboardingReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary offboarding review steps in order and keeps a trace of each.</summary>
public sealed class PrimaryOffboardingReviewEngine(List<IPrimaryOffboardingReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryOffboardingReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryOffboardingReviewEngine DefaultPrimaryOffboardingReviewEngine()
    {
        return new PrimaryOffboardingReviewEngine(
            new List<IPrimaryOffboardingReviewModifier>
            {
                new PrimaryOffboardingReviewMinimumOffsetAdjuster(0.7),
                new PercentPrimaryOffboardingReviewAdjuster(-14.2),
                new CapPrimaryOffboardingReviewAdjuster(30414.0),
            },
            new List<string>(),
            2.3,
            30280.2);
    }

    public double Evaluate(PrimaryOffboardingReview item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryOffboardingReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
