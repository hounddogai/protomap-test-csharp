namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary action plan mapping calculation.</summary>
public interface IPrimaryActionPlanMappingModifier
{
    string Describe();
    double Adjust(PrimaryActionPlanMapping item, double valueValue);
}

public sealed record MinimumPrimaryActionPlanMappingAdjuster(double Minimum) : IPrimaryActionPlanMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryActionPlanMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPrimaryActionPlanMappingAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryActionPlanMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryActionPlanMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentPrimaryActionPlanMappingAdjuster(Dictionary<string, double> Factors) : IPrimaryActionPlanMappingModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PrimaryActionPlanMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PrimaryActionPlanMappingFteOffsetAdjuster(double Weight) : IPrimaryActionPlanMappingModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(PrimaryActionPlanMapping item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredPrimaryActionPlanMappingAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryActionPlanMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryActionPlanMapping item, double valueValue)
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

public sealed record CapPrimaryActionPlanMappingAdjuster(double Cap) : IPrimaryActionPlanMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryActionPlanMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary action plan mapping steps in order and keeps a trace of each.</summary>
public sealed class PrimaryActionPlanMappingEngine(List<IPrimaryActionPlanMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryActionPlanMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryActionPlanMappingEngine DefaultPrimaryActionPlanMappingEngine(Registry flags)
    {
        return new PrimaryActionPlanMappingEngine(
            new List<IPrimaryActionPlanMappingModifier>
            {
                new MinimumPrimaryActionPlanMappingAdjuster(40.2),
                new FlaggedPrimaryActionPlanMappingAdjuster(flags, "primary-action-plan-mapping-strict", -5.2),
                new SegmentPrimaryActionPlanMappingAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.92, ["west"] = 0.83 }),
                new PrimaryActionPlanMappingFteOffsetAdjuster(0.47),
                new TieredPrimaryActionPlanMappingAdjuster(
                    new List<double> { 940.0, 4313.0, 6996.0, 7541.0 },
                    new List<double> { 1.19, 0.96, 0.8, 1.08 }),
                new CapPrimaryActionPlanMappingAdjuster(34788.0),
            },
            new List<string>(),
            1.9,
            11905.1);
    }

    public double Run(PrimaryActionPlanMapping item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryActionPlanMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
