namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge assignment calculation.</summary>
public interface IAccessBadgeAssignmentAdjuster
{
    string Describe();
    double Adjust(AccessBadgeAssignment item, double valueValue);
}

public sealed record PercentAccessBadgeAssignmentAdjuster(double Percent) : IAccessBadgeAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAccessBadgeAssignmentAdjuster(Dictionary<string, double> Factors) : IAccessBadgeAssignmentAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AccessBadgeAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAccessBadgeAssignmentAdjuster(double Cap) : IAccessBadgeAssignmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAccessBadgeAssignmentAdjuster(double Step) : IAccessBadgeAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAccessBadgeAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccessBadgeAssignmentAdjuster(double Minimum) : IAccessBadgeAssignmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the access badge assignment steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeAssignmentEngine(List<IAccessBadgeAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeAssignmentEngine DefaultAccessBadgeAssignmentEngine(Registry flags)
    {
        return new AccessBadgeAssignmentEngine(
            new List<IAccessBadgeAssignmentAdjuster>
            {
                new PercentAccessBadgeAssignmentAdjuster(-21.9),
                new NameAccessBadgeAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.71, ["retail"] = 0.95 }),
                new CapAccessBadgeAssignmentAdjuster(6494.0),
                new RoundingAccessBadgeAssignmentAdjuster(0.5),
                new FlaggedAccessBadgeAssignmentAdjuster(flags, "access-badge-assignment-lenient", -0.6),
                new MinimumAccessBadgeAssignmentAdjuster(29.8),
            },
            new List<string>(),
            0.4,
            13464.6);
    }

    public double Run(AccessBadgeAssignment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
