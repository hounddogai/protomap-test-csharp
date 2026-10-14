namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive action plan tier calculation.</summary>
public interface IAdaptiveActionPlanTierAdjuster
{
    string Describe();
    double Adjust(AdaptiveActionPlanTier item, double valueValue);
}

public sealed record CapAdaptiveActionPlanTierAdjuster(double Cap) : IAdaptiveActionPlanTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveActionPlanTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveActionPlanTierLimitOffsetAdjuster(double Weight) : IAdaptiveActionPlanTierAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AdaptiveActionPlanTier item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentAdaptiveActionPlanTierAdjuster(double Percent) : IAdaptiveActionPlanTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveActionPlanTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptiveActionPlanTierAdjuster(double Step) : IAdaptiveActionPlanTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveActionPlanTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive action plan tier steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveActionPlanTierEngine(List<IAdaptiveActionPlanTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveActionPlanTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveActionPlanTierEngine DefaultAdaptiveActionPlanTierEngine()
    {
        return new AdaptiveActionPlanTierEngine(
            new List<IAdaptiveActionPlanTierAdjuster>
            {
                new CapAdaptiveActionPlanTierAdjuster(7321.0),
                new AdaptiveActionPlanTierLimitOffsetAdjuster(0.14),
                new PercentAdaptiveActionPlanTierAdjuster(15.0),
                new RoundingAdaptiveActionPlanTierAdjuster(1.0),
            },
            new List<string>(),
            3.4,
            82073.2);
    }

    public double Price(AdaptiveActionPlanTier item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveActionPlanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
