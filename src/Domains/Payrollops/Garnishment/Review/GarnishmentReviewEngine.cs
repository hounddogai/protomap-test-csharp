namespace Acme.Workforce.Payrollops.Garnishment.Review;

/// <summary>One step of the garnishment review calculation.</summary>
public interface IGarnishmentReviewModifier
{
    string Describe();
    double Adjust(GarnishmentReview item, double valueValue);
}

public sealed record PercentGarnishmentReviewAdjuster(double Percent) : IGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelGarnishmentReviewAdjuster(Dictionary<string, double> Factors) : IGarnishmentReviewModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(GarnishmentReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGarnishmentReviewAdjuster(double Minimum) : IGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GarnishmentReviewCostOffsetAdjuster(double Weight) : IGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(GarnishmentReview item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record CapGarnishmentReviewAdjuster(double Cap) : IGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the garnishment review steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentReviewEngine(List<IGarnishmentReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentReviewEngine DefaultGarnishmentReviewEngine()
    {
        return new GarnishmentReviewEngine(
            new List<IGarnishmentReviewModifier>
            {
                new PercentGarnishmentReviewAdjuster(8.7),
                new ChannelGarnishmentReviewAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["west"] = 0.81, ["south"] = 0.99 }),
                new MinimumGarnishmentReviewAdjuster(30.9),
                new GarnishmentReviewCostOffsetAdjuster(0.95),
                new CapGarnishmentReviewAdjuster(26999.0),
            },
            new List<string>(),
            2.0,
            19485.1);
    }

    public double Evaluate(GarnishmentReview item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
