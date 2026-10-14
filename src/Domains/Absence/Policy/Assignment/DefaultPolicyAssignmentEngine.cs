namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default policy assignment calculation.</summary>
public interface IDefaultPolicyAssignmentModifier
{
    string Describe();
    double Adjust(DefaultPolicyAssignment item, double valueValue);
}

public sealed record FlaggedDefaultPolicyAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPolicyAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultPolicyAssignmentAdjuster(double Percent) : IDefaultPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPolicyAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameDefaultPolicyAssignmentAdjuster(Dictionary<string, double> Factors) : IDefaultPolicyAssignmentModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DefaultPolicyAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultPolicyAssignmentAdjuster(double Step) : IDefaultPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPolicyAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultPolicyAssignmentAdjuster(double Minimum) : IDefaultPolicyAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPolicyAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default policy assignment steps in order and keeps a trace of each.</summary>
public sealed class DefaultPolicyAssignmentEngine(List<IDefaultPolicyAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPolicyAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPolicyAssignmentEngine DefaultDefaultPolicyAssignmentEngine(Registry flags)
    {
        return new DefaultPolicyAssignmentEngine(
            new List<IDefaultPolicyAssignmentModifier>
            {
                new FlaggedDefaultPolicyAssignmentAdjuster(flags, "default-policy-assignment-tiered", -1.2),
                new PercentDefaultPolicyAssignmentAdjuster(-21.8),
                new NameDefaultPolicyAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.26, ["retail"] = 0.81, ["east"] = 1.21 }),
                new RoundingDefaultPolicyAssignmentAdjuster(0.5),
                new MinimumDefaultPolicyAssignmentAdjuster(24.1),
            },
            new List<string>(),
            1.5,
            27762.8);
    }

    public double Compute(DefaultPolicyAssignment item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPolicyAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
