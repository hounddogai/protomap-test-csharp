namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive action plan milestone calculation.</summary>
public interface IAdaptiveActionPlanMilestoneAdjuster
{
    string Describe();
    double Adjust(AdaptiveActionPlanMilestone item, double valueValue);
}

public sealed record AdaptiveActionPlanMilestoneStartsAtOffsetAdjuster(double Weight) : IAdaptiveActionPlanMilestoneAdjuster
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(AdaptiveActionPlanMilestone item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record TieredAdaptiveActionPlanMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveActionPlanMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveActionPlanMilestone item, double valueValue)
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

public sealed record PercentAdaptiveActionPlanMilestoneAdjuster(double Percent) : IAdaptiveActionPlanMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveActionPlanMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceAdaptiveActionPlanMilestoneAdjuster(Dictionary<string, double> Factors) : IAdaptiveActionPlanMilestoneAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdaptiveActionPlanMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive action plan milestone steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveActionPlanMilestoneEngine(List<IAdaptiveActionPlanMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveActionPlanMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveActionPlanMilestoneEngine DefaultAdaptiveActionPlanMilestoneEngine()
    {
        return new AdaptiveActionPlanMilestoneEngine(
            new List<IAdaptiveActionPlanMilestoneAdjuster>
            {
                new AdaptiveActionPlanMilestoneStartsAtOffsetAdjuster(1.09),
                new TieredAdaptiveActionPlanMilestoneAdjuster(
                    new List<double> { 906.0, 7275.0 },
                    new List<double> { 0.97, 0.82 }),
                new PercentAdaptiveActionPlanMilestoneAdjuster(20.4),
                new SourceAdaptiveActionPlanMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.78, ["online"] = 1.15, ["south"] = 0.91 }),
            },
            new List<string>(),
            3.2,
            63837.3);
    }

    public double Price(AdaptiveActionPlanMilestone item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveActionPlanMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
