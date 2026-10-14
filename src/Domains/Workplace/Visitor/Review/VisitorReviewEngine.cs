namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor review calculation.</summary>
public interface IVisitorReviewFactor
{
    string Describe();
    double Adjust(VisitorReview item, double valueValue);
}

public sealed record MinimumVisitorReviewAdjuster(double Minimum) : IVisitorReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingVisitorReviewAdjuster(double Step) : IVisitorReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedVisitorReviewAdjuster(Registry Flags, string Flag, double Percent) : IVisitorReviewFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record VisitorReviewPriorityOffsetAdjuster(double Weight) : IVisitorReviewFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(VisitorReview item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the visitor review steps in order and keeps a trace of each.</summary>
public sealed class VisitorReviewEngine(List<IVisitorReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorReviewEngine DefaultVisitorReviewEngine(Registry flags)
    {
        return new VisitorReviewEngine(
            new List<IVisitorReviewFactor>
            {
                new MinimumVisitorReviewAdjuster(11.2),
                new RoundingVisitorReviewAdjuster(1.0),
                new FlaggedVisitorReviewAdjuster(flags, "visitor-review-rolling", 0.7),
                new VisitorReviewPriorityOffsetAdjuster(0.49),
            },
            new List<string>(),
            4.0,
            83727.0);
    }

    public double Evaluate(VisitorReview item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
