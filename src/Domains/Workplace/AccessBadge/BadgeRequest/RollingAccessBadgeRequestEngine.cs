namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling access badge request calculation.</summary>
public interface IRollingAccessBadgeRequestStep
{
    string Describe();
    double Adjust(RollingAccessBadgeRequest item, double valueValue);
}

public sealed record MinimumRollingAccessBadgeRequestAdjuster(double Minimum) : IRollingAccessBadgeRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAccessBadgeRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRollingAccessBadgeRequestAdjuster(double Cap) : IRollingAccessBadgeRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAccessBadgeRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingAccessBadgeRequestAdjuster(Registry Flags, string Flag, double Percent) : IRollingAccessBadgeRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingAccessBadgeRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingAccessBadgeRequestAdjuster(double Step) : IRollingAccessBadgeRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingAccessBadgeRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRollingAccessBadgeRequestAdjuster(List<double> Bounds, List<double> Rates) : IRollingAccessBadgeRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingAccessBadgeRequest item, double valueValue)
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

/// <summary>Runs the rolling access badge request steps in order and keeps a trace of each.</summary>
public sealed class RollingAccessBadgeRequestEngine(List<IRollingAccessBadgeRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAccessBadgeRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAccessBadgeRequestEngine DefaultRollingAccessBadgeRequestEngine(Registry flags)
    {
        return new RollingAccessBadgeRequestEngine(
            new List<IRollingAccessBadgeRequestStep>
            {
                new MinimumRollingAccessBadgeRequestAdjuster(10.3),
                new CapRollingAccessBadgeRequestAdjuster(47482.0),
                new FlaggedRollingAccessBadgeRequestAdjuster(flags, "rolling-access-badge-request-legacy", 6.6),
                new RoundingRollingAccessBadgeRequestAdjuster(0.1),
                new TieredRollingAccessBadgeRequestAdjuster(
                    new List<double> { 1614.0, 3094.0, 5553.0, 8431.0 },
                    new List<double> { 0.89, 0.81, 1.11, 0.86 }),
            },
            new List<string>(),
            4.7,
            6903.5);
    }

    public double Run(RollingAccessBadgeRequest item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAccessBadgeRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
