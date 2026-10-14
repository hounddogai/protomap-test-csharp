namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the leave request group calculation.</summary>
public interface ILeaveRequestGroupStep
{
    string Describe();
    double Adjust(LeaveRequestGroup item, double valueValue);
}

public sealed record LeaveRequestGroupUpdatedAtOffsetAdjuster(double Weight) : ILeaveRequestGroupStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LeaveRequestGroup item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record MinimumLeaveRequestGroupAdjuster(double Minimum) : ILeaveRequestGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameLeaveRequestGroupAdjuster(Dictionary<string, double> Factors) : ILeaveRequestGroupStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LeaveRequestGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLeaveRequestGroupAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLeaveRequestGroupAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestGroup item, double valueValue)
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

public sealed record PercentLeaveRequestGroupAdjuster(double Percent) : ILeaveRequestGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request group steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestGroupEngine(List<ILeaveRequestGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestGroupEngine DefaultLeaveRequestGroupEngine(Registry flags)
    {
        return new LeaveRequestGroupEngine(
            new List<ILeaveRequestGroupStep>
            {
                new LeaveRequestGroupUpdatedAtOffsetAdjuster(0.51),
                new MinimumLeaveRequestGroupAdjuster(15.3),
                new NameLeaveRequestGroupAdjuster(new Dictionary<string, double> { ["online"] = 0.81 }),
                new FlaggedLeaveRequestGroupAdjuster(flags, "leave-request-group-global", -3.8),
                new TieredLeaveRequestGroupAdjuster(
                    new List<double> { 1910.0, 2328.0, 5154.0, 5209.0 },
                    new List<double> { 0.87, 1.03, 0.99, 0.86 }),
                new PercentLeaveRequestGroupAdjuster(13.8),
            },
            new List<string>(),
            4.3,
            39866.7);
    }

    public double Compute(LeaveRequestGroup item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
