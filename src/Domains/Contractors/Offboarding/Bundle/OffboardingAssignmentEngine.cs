namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding assignment calculation.</summary>
public interface IOffboardingAssignmentStep
{
    string Describe();
    double Adjust(OffboardingAssignment item, double valueValue);
}

public sealed record FlaggedOffboardingAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingOffboardingAssignmentAdjuster(double Step) : IOffboardingAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelOffboardingAssignmentAdjuster(Dictionary<string, double> Factors) : IOffboardingAssignmentStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OffboardingAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the offboarding assignment steps in order and keeps a trace of each.</summary>
public sealed class OffboardingAssignmentEngine(List<IOffboardingAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingAssignmentEngine DefaultOffboardingAssignmentEngine(Registry flags)
    {
        return new OffboardingAssignmentEngine(
            new List<IOffboardingAssignmentStep>
            {
                new FlaggedOffboardingAssignmentAdjuster(flags, "offboarding-assignment-rolling", 4.9),
                new RoundingOffboardingAssignmentAdjuster(0.5),
                new LabelOffboardingAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.84 }),
            },
            new List<string>(),
            2.7,
            32414.1);
    }

    public double Run(OffboardingAssignment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
