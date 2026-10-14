namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default exception review calculation.</summary>
public interface IDefaultExceptionReviewModifier
{
    string Describe();
    double Adjust(DefaultExceptionReview item, double valueValue);
}

public sealed record RoundingDefaultExceptionReviewAdjuster(double Step) : IDefaultExceptionReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultExceptionReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamDefaultExceptionReviewAdjuster(Dictionary<string, double> Factors) : IDefaultExceptionReviewModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DefaultExceptionReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultExceptionReviewAdjuster(List<double> Bounds, List<double> Rates) : IDefaultExceptionReviewModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultExceptionReview item, double valueValue)
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

public sealed record DefaultExceptionReviewCreatedAtOffsetAdjuster(double Weight) : IDefaultExceptionReviewModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(DefaultExceptionReview item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the default exception review steps in order and keeps a trace of each.</summary>
public sealed class DefaultExceptionReviewEngine(List<IDefaultExceptionReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultExceptionReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultExceptionReviewEngine DefaultDefaultExceptionReviewEngine()
    {
        return new DefaultExceptionReviewEngine(
            new List<IDefaultExceptionReviewModifier>
            {
                new RoundingDefaultExceptionReviewAdjuster(0.1),
                new TeamDefaultExceptionReviewAdjuster(new Dictionary<string, double> { ["retail"] = 1.02 }),
                new TieredDefaultExceptionReviewAdjuster(
                    new List<double> { 7653.0, 9150.0 },
                    new List<double> { 1.06, 0.95 }),
                new DefaultExceptionReviewCreatedAtOffsetAdjuster(1.24),
            },
            new List<string>(),
            4.9,
            89029.9);
    }

    public double Compute(DefaultExceptionReview item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultExceptionReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
