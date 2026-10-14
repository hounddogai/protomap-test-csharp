namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive warranty milestone calculation.</summary>
public interface IAdaptiveWarrantyMilestoneStep
{
    string Describe();
    double Adjust(AdaptiveWarrantyMilestone item, double valueValue);
}

public sealed record CapAdaptiveWarrantyMilestoneAdjuster(double Cap) : IAdaptiveWarrantyMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveWarrantyMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveWarrantyMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveWarrantyMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveWarrantyMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptiveWarrantyMilestoneAdjuster(double Percent) : IAdaptiveWarrantyMilestoneStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveWarrantyMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveWarrantyMilestoneLimitOffsetAdjuster(double Weight) : IAdaptiveWarrantyMilestoneStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AdaptiveWarrantyMilestone item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record UnitAdaptiveWarrantyMilestoneAdjuster(Dictionary<string, double> Factors) : IAdaptiveWarrantyMilestoneStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AdaptiveWarrantyMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveWarrantyMilestoneAdjuster(double Step) : IAdaptiveWarrantyMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveWarrantyMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive warranty milestone steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveWarrantyMilestoneEngine(List<IAdaptiveWarrantyMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveWarrantyMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveWarrantyMilestoneEngine DefaultAdaptiveWarrantyMilestoneEngine(Registry flags)
    {
        return new AdaptiveWarrantyMilestoneEngine(
            new List<IAdaptiveWarrantyMilestoneStep>
            {
                new CapAdaptiveWarrantyMilestoneAdjuster(36064.0),
                new FlaggedAdaptiveWarrantyMilestoneAdjuster(flags, "adaptive-warranty-milestone-pending", 3.8),
                new PercentAdaptiveWarrantyMilestoneAdjuster(-7.4),
                new AdaptiveWarrantyMilestoneLimitOffsetAdjuster(1.6),
                new UnitAdaptiveWarrantyMilestoneAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.01, ["retail"] = 1.36 }),
                new RoundingAdaptiveWarrantyMilestoneAdjuster(1.0),
            },
            new List<string>(),
            3.3,
            88934.4);
    }

    public double Run(AdaptiveWarrantyMilestone item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveWarrantyMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
