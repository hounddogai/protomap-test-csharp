namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive action plan slot calculation.</summary>
public interface IAdaptiveActionPlanSlotAdjuster
{
    string Describe();
    double Adjust(AdaptiveActionPlanSlot item, double valueValue);
}

public sealed record MinimumAdaptiveActionPlanSlotAdjuster(double Minimum) : IAdaptiveActionPlanSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveActionPlanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdaptiveActionPlanSlotAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveActionPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveActionPlanSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptiveActionPlanSlotAdjuster(double Percent) : IAdaptiveActionPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveActionPlanSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveActionPlanSlotAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveActionPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveActionPlanSlot item, double valueValue)
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

public sealed record CapAdaptiveActionPlanSlotAdjuster(double Cap) : IAdaptiveActionPlanSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveActionPlanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveActionPlanSlotScoreOffsetAdjuster(double Weight) : IAdaptiveActionPlanSlotAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AdaptiveActionPlanSlot item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the adaptive action plan slot steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveActionPlanSlotEngine(List<IAdaptiveActionPlanSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveActionPlanSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveActionPlanSlotEngine DefaultAdaptiveActionPlanSlotEngine(Registry flags)
    {
        return new AdaptiveActionPlanSlotEngine(
            new List<IAdaptiveActionPlanSlotAdjuster>
            {
                new MinimumAdaptiveActionPlanSlotAdjuster(26.8),
                new FlaggedAdaptiveActionPlanSlotAdjuster(flags, "adaptive-action-plan-slot-adaptive", 8.1),
                new PercentAdaptiveActionPlanSlotAdjuster(-12.6),
                new TieredAdaptiveActionPlanSlotAdjuster(
                    new List<double> { 2371.0, 7007.0, 7298.0 },
                    new List<double> { 1.01, 0.85, 0.84 }),
                new CapAdaptiveActionPlanSlotAdjuster(15692.0),
                new AdaptiveActionPlanSlotScoreOffsetAdjuster(1.89),
            },
            new List<string>(),
            3.7,
            98343.2);
    }

    public double Price(AdaptiveActionPlanSlot item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveActionPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
