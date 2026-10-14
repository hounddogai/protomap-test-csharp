namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict deduction milestone calculation.</summary>
public interface IStrictDeductionMilestoneStep
{
    string Describe();
    double Adjust(StrictDeductionMilestone item, double valueValue);
}

public sealed record MinimumStrictDeductionMilestoneAdjuster(double Minimum) : IStrictDeductionMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictDeductionMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedStrictDeductionMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IStrictDeductionMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictDeductionMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameStrictDeductionMilestoneAdjuster(Dictionary<string, double> Factors) : IStrictDeductionMilestoneStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictDeductionMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingStrictDeductionMilestoneAdjuster(double Step) : IStrictDeductionMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictDeductionMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict deduction milestone steps in order and keeps a trace of each.</summary>
public sealed class StrictDeductionMilestoneEngine(List<IStrictDeductionMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictDeductionMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictDeductionMilestoneEngine DefaultStrictDeductionMilestoneEngine(Registry flags)
    {
        return new StrictDeductionMilestoneEngine(
            new List<IStrictDeductionMilestoneStep>
            {
                new MinimumStrictDeductionMilestoneAdjuster(16.1),
                new FlaggedStrictDeductionMilestoneAdjuster(flags, "strict-deduction-milestone-rolling", -8.9),
                new NameStrictDeductionMilestoneAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.19, ["north"] = 1.24 }),
                new RoundingStrictDeductionMilestoneAdjuster(0.1),
            },
            new List<string>(),
            2.6,
            25830.4);
    }

    public double Run(StrictDeductionMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictDeductionMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
