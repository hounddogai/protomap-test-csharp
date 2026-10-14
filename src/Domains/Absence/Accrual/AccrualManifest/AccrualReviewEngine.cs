namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual review calculation.</summary>
public interface IAccrualReviewFactor
{
    string Describe();
    double Adjust(AccrualReview item, double valueValue);
}

public sealed record MinimumAccrualReviewAdjuster(double Minimum) : IAccrualReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceAccrualReviewAdjuster(Dictionary<string, double> Factors) : IAccrualReviewFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AccrualReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAccrualReviewAdjuster(double Step) : IAccrualReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAccrualReviewAdjuster(double Percent) : IAccrualReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccrualReviewHoursOffsetAdjuster(double Weight) : IAccrualReviewFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(AccrualReview item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the accrual review steps in order and keeps a trace of each.</summary>
public sealed class AccrualReviewEngine(List<IAccrualReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualReviewEngine DefaultAccrualReviewEngine()
    {
        return new AccrualReviewEngine(
            new List<IAccrualReviewFactor>
            {
                new MinimumAccrualReviewAdjuster(23.6),
                new SourceAccrualReviewAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.37, ["central"] = 1.02 }),
                new RoundingAccrualReviewAdjuster(0.05),
                new PercentAccrualReviewAdjuster(-7.4),
                new AccrualReviewHoursOffsetAdjuster(1.32),
            },
            new List<string>(),
            4.1,
            8084.6);
    }

    public double Compute(AccrualReview item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
