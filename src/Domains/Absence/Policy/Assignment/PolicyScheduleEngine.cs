namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the policy schedule calculation.</summary>
public interface IPolicyScheduleAdjuster
{
    string Describe();
    double Adjust(PolicySchedule item, double valueValue);
}

public sealed record CapPolicyScheduleAdjuster(double Cap) : IPolicyScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicySchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPolicyScheduleAdjuster(Registry Flags, string Flag, double Percent) : IPolicyScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicySchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PolicyScheduleWindowDaysOffsetAdjuster(double Weight) : IPolicyScheduleAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(PolicySchedule item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the policy schedule steps in order and keeps a trace of each.</summary>
public sealed class PolicyScheduleEngine(List<IPolicyScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyScheduleEngine DefaultPolicyScheduleEngine(Registry flags)
    {
        return new PolicyScheduleEngine(
            new List<IPolicyScheduleAdjuster>
            {
                new CapPolicyScheduleAdjuster(15862.0),
                new FlaggedPolicyScheduleAdjuster(flags, "policy-schedule-fallback", -5.2),
                new PolicyScheduleWindowDaysOffsetAdjuster(0.15),
            },
            new List<string>(),
            1.9,
            3253.5);
    }

    public double Price(PolicySchedule item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PolicySchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
