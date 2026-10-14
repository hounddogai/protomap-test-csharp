namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement plan calculation.</summary>
public interface IEngagementPlanModifier
{
    string Describe();
    double Adjust(EngagementPlan item, double valueValue);
}

public sealed record TieredEngagementPlanAdjuster(List<double> Bounds, List<double> Rates) : IEngagementPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementPlan item, double valueValue)
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

public sealed record EngagementPlanQuantityOffsetAdjuster(double Weight) : IEngagementPlanModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(EngagementPlan item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record MinimumEngagementPlanAdjuster(double Minimum) : IEngagementPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedEngagementPlanAdjuster(Registry Flags, string Flag, double Percent) : IEngagementPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEngagementPlanAdjuster(double Step) : IEngagementPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapEngagementPlanAdjuster(double Cap) : IEngagementPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the engagement plan steps in order and keeps a trace of each.</summary>
public sealed class EngagementPlanEngine(List<IEngagementPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementPlanEngine DefaultEngagementPlanEngine(Registry flags)
    {
        return new EngagementPlanEngine(
            new List<IEngagementPlanModifier>
            {
                new TieredEngagementPlanAdjuster(
                    new List<double> { 3600.0, 4981.0, 5655.0 },
                    new List<double> { 1.15, 0.98, 1.03 }),
                new EngagementPlanQuantityOffsetAdjuster(0.96),
                new MinimumEngagementPlanAdjuster(12.7),
                new FlaggedEngagementPlanAdjuster(flags, "engagement-plan-tiered", -2.5),
                new RoundingEngagementPlanAdjuster(0.1),
                new CapEngagementPlanAdjuster(26068.0),
            },
            new List<string>(),
            2.7,
            52619.3);
    }

    public double Run(EngagementPlan item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
