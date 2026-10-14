namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive position plan calculation.</summary>
public interface IAdaptivePositionPlanModifier
{
    string Describe();
    double Adjust(AdaptivePositionPlan item, double valueValue);
}

public sealed record CapAdaptivePositionPlanAdjuster(double Cap) : IAdaptivePositionPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptivePositionPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptivePositionPlanAdjuster(double Percent) : IAdaptivePositionPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePositionPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptivePositionPlanAdjuster(double Minimum) : IAdaptivePositionPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePositionPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptivePositionPlanAdjuster(List<double> Bounds, List<double> Rates) : IAdaptivePositionPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptivePositionPlan item, double valueValue)
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

public sealed record FlaggedAdaptivePositionPlanAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePositionPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePositionPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAdaptivePositionPlanAdjuster(double Step) : IAdaptivePositionPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptivePositionPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive position plan steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePositionPlanEngine(List<IAdaptivePositionPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePositionPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePositionPlanEngine DefaultAdaptivePositionPlanEngine(Registry flags)
    {
        return new AdaptivePositionPlanEngine(
            new List<IAdaptivePositionPlanModifier>
            {
                new CapAdaptivePositionPlanAdjuster(10312.0),
                new PercentAdaptivePositionPlanAdjuster(-3.0),
                new MinimumAdaptivePositionPlanAdjuster(5.7),
                new TieredAdaptivePositionPlanAdjuster(
                    new List<double> { 2729.0, 4066.0 },
                    new List<double> { 0.83, 1.16 }),
                new FlaggedAdaptivePositionPlanAdjuster(flags, "adaptive-position-plan-batched", 1.3),
                new RoundingAdaptivePositionPlanAdjuster(5.0),
            },
            new List<string>(),
            0.0,
            77119.5);
    }

    public double Price(AdaptivePositionPlan item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePositionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
