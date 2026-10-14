namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal feedback estimate calculation.</summary>
public interface ISeasonalFeedbackEstimateModifier
{
    string Describe();
    double Adjust(SeasonalFeedbackEstimate item, double valueValue);
}

public sealed record LabelSeasonalFeedbackEstimateAdjuster(Dictionary<string, double> Factors) : ISeasonalFeedbackEstimateModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalFeedbackEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalFeedbackEstimateAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalFeedbackEstimate item, double valueValue)
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

public sealed record FlaggedSeasonalFeedbackEstimateAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalFeedbackEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSeasonalFeedbackEstimateAdjuster(double Step) : ISeasonalFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalFeedbackEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalFeedbackEstimateAdjuster(double Cap) : ISeasonalFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalFeedbackEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalFeedbackEstimateEndsAtOffsetAdjuster(double Weight) : ISeasonalFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(SeasonalFeedbackEstimate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the seasonal feedback estimate steps in order and keeps a trace of each.</summary>
public sealed class SeasonalFeedbackEstimateEngine(List<ISeasonalFeedbackEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalFeedbackEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalFeedbackEstimateEngine DefaultSeasonalFeedbackEstimateEngine(Registry flags)
    {
        return new SeasonalFeedbackEstimateEngine(
            new List<ISeasonalFeedbackEstimateModifier>
            {
                new LabelSeasonalFeedbackEstimateAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.12, ["online"] = 1.32, ["central"] = 0.9 }),
                new TieredSeasonalFeedbackEstimateAdjuster(
                    new List<double> { 1429.0, 3150.0, 9231.0 },
                    new List<double> { 0.97, 0.91, 1.15 }),
                new FlaggedSeasonalFeedbackEstimateAdjuster(flags, "seasonal-feedback-estimate-tiered", -3.3),
                new RoundingSeasonalFeedbackEstimateAdjuster(0.1),
                new CapSeasonalFeedbackEstimateAdjuster(21401.0),
                new SeasonalFeedbackEstimateEndsAtOffsetAdjuster(1.54),
            },
            new List<string>(),
            3.4,
            14844.1);
    }

    public double Compute(SeasonalFeedbackEstimate item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalFeedbackEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
