namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the career path review calculation.</summary>
public interface ICareerPathReviewStep
{
    string Describe();
    double Adjust(CareerPathReview item, double valueValue);
}

public sealed record MinimumCareerPathReviewAdjuster(double Minimum) : ICareerPathReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCareerPathReviewAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathReviewStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelCareerPathReviewAdjuster(Dictionary<string, double> Factors) : ICareerPathReviewStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CareerPathReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCareerPathReviewAdjuster(double Percent) : ICareerPathReviewStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCareerPathReviewAdjuster(double Cap) : ICareerPathReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the career path review steps in order and keeps a trace of each.</summary>
public sealed class CareerPathReviewEngine(List<ICareerPathReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathReviewEngine DefaultCareerPathReviewEngine(Registry flags)
    {
        return new CareerPathReviewEngine(
            new List<ICareerPathReviewStep>
            {
                new MinimumCareerPathReviewAdjuster(8.4),
                new FlaggedCareerPathReviewAdjuster(flags, "career-path-review-weighted", 3.4),
                new LabelCareerPathReviewAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.34, ["west"] = 1.01, ["north"] = 1.34 }),
                new PercentCareerPathReviewAdjuster(4.0),
                new CapCareerPathReviewAdjuster(17356.0),
            },
            new List<string>(),
            1.6,
            42479.5);
    }

    public double Run(CareerPathReview item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
