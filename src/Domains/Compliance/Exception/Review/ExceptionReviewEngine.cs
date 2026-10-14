namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception review calculation.</summary>
public interface IExceptionReviewAdjuster
{
    string Describe();
    double Adjust(ExceptionReview item, double valueValue);
}

public sealed record PercentExceptionReviewAdjuster(double Percent) : IExceptionReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredExceptionReviewAdjuster(List<double> Bounds, List<double> Rates) : IExceptionReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionReview item, double valueValue)
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

public sealed record CapExceptionReviewAdjuster(double Cap) : IExceptionReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingExceptionReviewAdjuster(double Step) : IExceptionReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleExceptionReviewAdjuster(Dictionary<string, double> Factors) : IExceptionReviewAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ExceptionReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the exception review steps in order and keeps a trace of each.</summary>
public sealed class ExceptionReviewEngine(List<IExceptionReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionReviewEngine DefaultExceptionReviewEngine()
    {
        return new ExceptionReviewEngine(
            new List<IExceptionReviewAdjuster>
            {
                new PercentExceptionReviewAdjuster(21.9),
                new TieredExceptionReviewAdjuster(
                    new List<double> { 455.0, 2654.0, 4554.0, 6101.0 },
                    new List<double> { 1.19, 0.99, 1.14, 1.15 }),
                new CapExceptionReviewAdjuster(36669.0),
                new RoundingExceptionReviewAdjuster(0.25),
                new LocaleExceptionReviewAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["central"] = 0.92, ["online"] = 0.87 }),
            },
            new List<string>(),
            3.5,
            54588.2);
    }

    public double Compute(ExceptionReview item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
