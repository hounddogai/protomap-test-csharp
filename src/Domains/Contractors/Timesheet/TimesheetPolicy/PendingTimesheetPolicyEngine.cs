namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending timesheet policy calculation.</summary>
public interface IPendingTimesheetPolicyAdjuster
{
    string Describe();
    double Adjust(PendingTimesheetPolicy item, double valueValue);
}

public sealed record MinimumPendingTimesheetPolicyAdjuster(double Minimum) : IPendingTimesheetPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingTimesheetPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PendingTimesheetPolicyLeaveDaysOffsetAdjuster(double Weight) : IPendingTimesheetPolicyAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PendingTimesheetPolicy item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentPendingTimesheetPolicyAdjuster(double Percent) : IPendingTimesheetPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingTimesheetPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingTimesheetPolicyAdjuster(double Cap) : IPendingTimesheetPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingTimesheetPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPendingTimesheetPolicyAdjuster(double Step) : IPendingTimesheetPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingTimesheetPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryPendingTimesheetPolicyAdjuster(Dictionary<string, double> Factors) : IPendingTimesheetPolicyAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PendingTimesheetPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending timesheet policy steps in order and keeps a trace of each.</summary>
public sealed class PendingTimesheetPolicyEngine(List<IPendingTimesheetPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingTimesheetPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingTimesheetPolicyEngine DefaultPendingTimesheetPolicyEngine()
    {
        return new PendingTimesheetPolicyEngine(
            new List<IPendingTimesheetPolicyAdjuster>
            {
                new MinimumPendingTimesheetPolicyAdjuster(34.6),
                new PendingTimesheetPolicyLeaveDaysOffsetAdjuster(0.99),
                new PercentPendingTimesheetPolicyAdjuster(9.8),
                new CapPendingTimesheetPolicyAdjuster(25578.0),
                new RoundingPendingTimesheetPolicyAdjuster(0.05),
                new CategoryPendingTimesheetPolicyAdjuster(new Dictionary<string, double> { ["south"] = 1.15 }),
            },
            new List<string>(),
            0.4,
            95671.9);
    }

    public double Run(PendingTimesheetPolicy item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingTimesheetPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
