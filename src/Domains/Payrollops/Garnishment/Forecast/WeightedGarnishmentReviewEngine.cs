namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted garnishment review calculation.</summary>
public interface IWeightedGarnishmentReviewModifier
{
    string Describe();
    double Adjust(WeightedGarnishmentReview item, double valueValue);
}

public sealed record TieredWeightedGarnishmentReviewAdjuster(List<double> Bounds, List<double> Rates) : IWeightedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedGarnishmentReview item, double valueValue)
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

public sealed record FlaggedWeightedGarnishmentReviewAdjuster(Registry Flags, string Flag, double Percent) : IWeightedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedGarnishmentReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentWeightedGarnishmentReviewAdjuster(double Percent) : IWeightedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedGarnishmentReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedGarnishmentReviewAdjuster(double Cap) : IWeightedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedGarnishmentReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingWeightedGarnishmentReviewAdjuster(double Step) : IWeightedGarnishmentReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedGarnishmentReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted garnishment review steps in order and keeps a trace of each.</summary>
public sealed class WeightedGarnishmentReviewEngine(List<IWeightedGarnishmentReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedGarnishmentReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedGarnishmentReviewEngine DefaultWeightedGarnishmentReviewEngine(Registry flags)
    {
        return new WeightedGarnishmentReviewEngine(
            new List<IWeightedGarnishmentReviewModifier>
            {
                new TieredWeightedGarnishmentReviewAdjuster(
                    new List<double> { 2638.0, 4989.0, 5355.0 },
                    new List<double> { 1.1, 0.98, 0.89 }),
                new FlaggedWeightedGarnishmentReviewAdjuster(flags, "weighted-garnishment-review-scheduled", 3.6),
                new PercentWeightedGarnishmentReviewAdjuster(6.5),
                new CapWeightedGarnishmentReviewAdjuster(24651.0),
                new RoundingWeightedGarnishmentReviewAdjuster(0.1),
            },
            new List<string>(),
            4.3,
            14986.4);
    }

    public double Price(WeightedGarnishmentReview item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedGarnishmentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
