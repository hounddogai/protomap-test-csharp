namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty milestone calculation.</summary>
public interface IWarrantyMilestoneModifier
{
    string Describe();
    double Adjust(WarrantyMilestone item, double valueValue);
}

public sealed record FlaggedWarrantyMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWarrantyMilestoneAdjuster(double Step) : IWarrantyMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WarrantyMilestoneEndsAtOffsetAdjuster(double Weight) : IWarrantyMilestoneModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(WarrantyMilestone item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record MinimumWarrantyMilestoneAdjuster(double Minimum) : IWarrantyMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapWarrantyMilestoneAdjuster(double Cap) : IWarrantyMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWarrantyMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyMilestone item, double valueValue)
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

/// <summary>Runs the warranty milestone steps in order and keeps a trace of each.</summary>
public sealed class WarrantyMilestoneEngine(List<IWarrantyMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyMilestoneEngine DefaultWarrantyMilestoneEngine(Registry flags)
    {
        return new WarrantyMilestoneEngine(
            new List<IWarrantyMilestoneModifier>
            {
                new FlaggedWarrantyMilestoneAdjuster(flags, "warranty-milestone-global", 9.3),
                new RoundingWarrantyMilestoneAdjuster(0.05),
                new WarrantyMilestoneEndsAtOffsetAdjuster(0.69),
                new MinimumWarrantyMilestoneAdjuster(47.3),
                new CapWarrantyMilestoneAdjuster(49297.0),
                new TieredWarrantyMilestoneAdjuster(
                    new List<double> { 2928.0, 3608.0, 7217.0, 9175.0 },
                    new List<double> { 1.17, 1.17, 0.88, 1.19 }),
            },
            new List<string>(),
            2.3,
            54912.7);
    }

    public double Compute(WarrantyMilestone item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
