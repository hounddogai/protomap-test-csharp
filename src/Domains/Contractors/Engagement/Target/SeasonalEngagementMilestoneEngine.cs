namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal engagement milestone calculation.</summary>
public interface ISeasonalEngagementMilestoneAdjuster
{
    string Describe();
    double Adjust(SeasonalEngagementMilestone item, double valueValue);
}

public sealed record CapSeasonalEngagementMilestoneAdjuster(double Cap) : ISeasonalEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalEngagementMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalEngagementMilestoneAdjuster(double Step) : ISeasonalEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalEngagementMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SeasonalEngagementMilestoneRatingOffsetAdjuster(double Weight) : ISeasonalEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(SeasonalEngagementMilestone item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record TieredSeasonalEngagementMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalEngagementMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalEngagementMilestone item, double valueValue)
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

/// <summary>Runs the seasonal engagement milestone steps in order and keeps a trace of each.</summary>
public sealed class SeasonalEngagementMilestoneEngine(List<ISeasonalEngagementMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalEngagementMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalEngagementMilestoneEngine DefaultSeasonalEngagementMilestoneEngine()
    {
        return new SeasonalEngagementMilestoneEngine(
            new List<ISeasonalEngagementMilestoneAdjuster>
            {
                new CapSeasonalEngagementMilestoneAdjuster(5033.0),
                new RoundingSeasonalEngagementMilestoneAdjuster(0.5),
                new SeasonalEngagementMilestoneRatingOffsetAdjuster(1.4),
                new TieredSeasonalEngagementMilestoneAdjuster(
                    new List<double> { 3130.0, 9522.0 },
                    new List<double> { 0.9, 0.81 }),
            },
            new List<string>(),
            1.5,
            8285.8);
    }

    public double Price(SeasonalEngagementMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalEngagementMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
