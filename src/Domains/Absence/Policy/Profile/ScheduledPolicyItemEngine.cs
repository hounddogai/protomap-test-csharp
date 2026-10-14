namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled policy item calculation.</summary>
public interface IScheduledPolicyItemStep
{
    string Describe();
    double Adjust(ScheduledPolicyItem item, double valueValue);
}

public sealed record FlaggedScheduledPolicyItemAdjuster(Registry Flags, string Flag, double Percent) : IScheduledPolicyItemStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledPolicyItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledPolicyItemAdjuster(double Step) : IScheduledPolicyItemStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPolicyItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryScheduledPolicyItemAdjuster(Dictionary<string, double> Factors) : IScheduledPolicyItemStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ScheduledPolicyItem item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled policy item steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPolicyItemEngine(List<IScheduledPolicyItemStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPolicyItemStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPolicyItemEngine DefaultScheduledPolicyItemEngine(Registry flags)
    {
        return new ScheduledPolicyItemEngine(
            new List<IScheduledPolicyItemStep>
            {
                new FlaggedScheduledPolicyItemAdjuster(flags, "scheduled-policy-item-rolling", 1.7),
                new RoundingScheduledPolicyItemAdjuster(0.1),
                new CategoryScheduledPolicyItemAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.84, ["north"] = 1.39, ["east"] = 1.22 }),
            },
            new List<string>(),
            2.6,
            46773.4);
    }

    public double Price(ScheduledPolicyItem item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPolicyItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
