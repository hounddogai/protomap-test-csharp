namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the leave request quota calculation.</summary>
public interface ILeaveRequestQuotaModifier
{
    string Describe();
    double Adjust(LeaveRequestQuota item, double valueValue);
}

public sealed record NameLeaveRequestQuotaAdjuster(Dictionary<string, double> Factors) : ILeaveRequestQuotaModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LeaveRequestQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLeaveRequestQuotaAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLeaveRequestQuotaAdjuster(double Percent) : ILeaveRequestQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLeaveRequestQuotaAdjuster(double Cap) : ILeaveRequestQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLeaveRequestQuotaAdjuster(double Minimum) : ILeaveRequestQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the leave request quota steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestQuotaEngine(List<ILeaveRequestQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestQuotaEngine DefaultLeaveRequestQuotaEngine(Registry flags)
    {
        return new LeaveRequestQuotaEngine(
            new List<ILeaveRequestQuotaModifier>
            {
                new NameLeaveRequestQuotaAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.84, ["west"] = 0.97, ["central"] = 1.28 }),
                new FlaggedLeaveRequestQuotaAdjuster(flags, "leave-request-quota-legacy", 3.1),
                new PercentLeaveRequestQuotaAdjuster(-23.0),
                new CapLeaveRequestQuotaAdjuster(8146.0),
                new MinimumLeaveRequestQuotaAdjuster(4.3),
            },
            new List<string>(),
            1.0,
            55819.2);
    }

    public double Price(LeaveRequestQuota item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
