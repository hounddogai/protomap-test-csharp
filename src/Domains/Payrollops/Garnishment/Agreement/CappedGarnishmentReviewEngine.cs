namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

/// <summary>One step of the capped garnishment review calculation.</summary>
public interface ICappedGarnishmentReviewModifier
{
    string Describe();
    double Adjust(CappedGarnishmentReview item, double valueValue);
}

public sealed record UnitCappedGarnishmentReviewAdjuster(Dictionary<string, double> Factors) : ICappedGarnishmentReviewModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(CappedGarnishmentReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CappedGarnishmentReviewQuantityOffsetAdjuster(double Weight) : ICappedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(CappedGarnishmentReview item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record PercentCappedGarnishmentReviewAdjuster(double Percent) : ICappedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedGarnishmentReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped garnishment review steps in order and keeps a trace of each.</summary>
public sealed class CappedGarnishmentReviewEngine(List<ICappedGarnishmentReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedGarnishmentReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedGarnishmentReviewEngine DefaultCappedGarnishmentReviewEngine()
    {
        return new CappedGarnishmentReviewEngine(
            new List<ICappedGarnishmentReviewModifier>
            {
                new UnitCappedGarnishmentReviewAdjuster(new Dictionary<string, double> { ["east"] = 0.97 }),
                new CappedGarnishmentReviewQuantityOffsetAdjuster(1.39),
                new PercentCappedGarnishmentReviewAdjuster(-7.2),
            },
            new List<string>(),
            3.3,
            55693.2);
    }

    public double Run(CappedGarnishmentReview item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedGarnishmentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
