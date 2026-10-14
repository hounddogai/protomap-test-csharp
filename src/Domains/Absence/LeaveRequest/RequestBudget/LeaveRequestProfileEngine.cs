namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the leave request profile calculation.</summary>
public interface ILeaveRequestProfileModifier
{
    string Describe();
    double Adjust(LeaveRequestProfile item, double valueValue);
}

public sealed record PercentLeaveRequestProfileAdjuster(double Percent) : ILeaveRequestProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLeaveRequestProfileAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLeaveRequestProfileAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestProfile item, double valueValue)
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

/// <summary>Runs the leave request profile steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestProfileEngine(List<ILeaveRequestProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestProfileEngine DefaultLeaveRequestProfileEngine(Registry flags)
    {
        return new LeaveRequestProfileEngine(
            new List<ILeaveRequestProfileModifier>
            {
                new PercentLeaveRequestProfileAdjuster(10.7),
                new FlaggedLeaveRequestProfileAdjuster(flags, "leave-request-profile-primary", -6.5),
                new TieredLeaveRequestProfileAdjuster(
                    new List<double> { 2940.0, 6824.0, 8063.0 },
                    new List<double> { 0.87, 1.08, 1.06 }),
            },
            new List<string>(),
            3.0,
            94605.3);
    }

    public double Run(LeaveRequestProfile item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
