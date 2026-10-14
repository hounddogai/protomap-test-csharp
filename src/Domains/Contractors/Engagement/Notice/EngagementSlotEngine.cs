namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement slot calculation.</summary>
public interface IEngagementSlotAdjuster
{
    string Describe();
    double Adjust(EngagementSlot item, double valueValue);
}

public sealed record CapEngagementSlotAdjuster(double Cap) : IEngagementSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EngagementSlotWeightOffsetAdjuster(double Weight) : IEngagementSlotAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(EngagementSlot item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingEngagementSlotAdjuster(double Step) : IEngagementSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedEngagementSlotAdjuster(Registry Flags, string Flag, double Percent) : IEngagementSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumEngagementSlotAdjuster(double Minimum) : IEngagementSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredEngagementSlotAdjuster(List<double> Bounds, List<double> Rates) : IEngagementSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementSlot item, double valueValue)
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

/// <summary>Runs the engagement slot steps in order and keeps a trace of each.</summary>
public sealed class EngagementSlotEngine(List<IEngagementSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementSlotEngine DefaultEngagementSlotEngine(Registry flags)
    {
        return new EngagementSlotEngine(
            new List<IEngagementSlotAdjuster>
            {
                new CapEngagementSlotAdjuster(35190.0),
                new EngagementSlotWeightOffsetAdjuster(1.88),
                new RoundingEngagementSlotAdjuster(0.5),
                new FlaggedEngagementSlotAdjuster(flags, "engagement-slot-strict", 4.8),
                new MinimumEngagementSlotAdjuster(32.8),
                new TieredEngagementSlotAdjuster(
                    new List<double> { 2991.0, 4167.0 },
                    new List<double> { 0.91, 0.97 }),
            },
            new List<string>(),
            0.2,
            30657.4);
    }

    public double Run(EngagementSlot item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
