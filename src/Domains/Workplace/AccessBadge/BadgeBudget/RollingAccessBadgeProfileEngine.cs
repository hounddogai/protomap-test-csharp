namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling access badge profile calculation.</summary>
public interface IRollingAccessBadgeProfileStep
{
    string Describe();
    double Adjust(RollingAccessBadgeProfile item, double valueValue);
}

public sealed record CapRollingAccessBadgeProfileAdjuster(double Cap) : IRollingAccessBadgeProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAccessBadgeProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingAccessBadgeProfileAdjuster(Registry Flags, string Flag, double Percent) : IRollingAccessBadgeProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingAccessBadgeProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelRollingAccessBadgeProfileAdjuster(Dictionary<string, double> Factors) : IRollingAccessBadgeProfileStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RollingAccessBadgeProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRollingAccessBadgeProfileAdjuster(double Step) : IRollingAccessBadgeProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingAccessBadgeProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRollingAccessBadgeProfileAdjuster(List<double> Bounds, List<double> Rates) : IRollingAccessBadgeProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAccessBadgeProfile item, double valueValue)
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

public sealed record MinimumRollingAccessBadgeProfileAdjuster(double Minimum) : IRollingAccessBadgeProfileStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAccessBadgeProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling access badge profile steps in order and keeps a trace of each.</summary>
public sealed class RollingAccessBadgeProfileEngine(List<IRollingAccessBadgeProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAccessBadgeProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAccessBadgeProfileEngine DefaultRollingAccessBadgeProfileEngine(Registry flags)
    {
        return new RollingAccessBadgeProfileEngine(
            new List<IRollingAccessBadgeProfileStep>
            {
                new CapRollingAccessBadgeProfileAdjuster(2973.0),
                new FlaggedRollingAccessBadgeProfileAdjuster(flags, "rolling-access-badge-profile-strict", 3.3),
                new LabelRollingAccessBadgeProfileAdjuster(new Dictionary<string, double> { ["north"] = 1.31 }),
                new RoundingRollingAccessBadgeProfileAdjuster(5.0),
                new TieredRollingAccessBadgeProfileAdjuster(
                    new List<double> { 1324.0, 6793.0, 7463.0, 8062.0 },
                    new List<double> { 1.11, 0.99, 1.06, 1.16 }),
                new MinimumRollingAccessBadgeProfileAdjuster(8.4),
            },
            new List<string>(),
            3.0,
            87586.2);
    }

    public double Price(RollingAccessBadgeProfile item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAccessBadgeProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
