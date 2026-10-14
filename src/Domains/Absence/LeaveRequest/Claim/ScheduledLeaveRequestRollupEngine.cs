namespace Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>One step of the scheduled leave request rollup calculation.</summary>
public interface IScheduledLeaveRequestRollupStep
{
    string Describe();
    double Adjust(ScheduledLeaveRequestRollup item, double valueValue);
}

public sealed record MinimumScheduledLeaveRequestRollupAdjuster(double Minimum) : IScheduledLeaveRequestRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledLeaveRequestRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledLeaveRequestRollupExpiresAtOffsetAdjuster(double Weight) : IScheduledLeaveRequestRollupStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ScheduledLeaveRequestRollup item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentScheduledLeaveRequestRollupAdjuster(double Percent) : IScheduledLeaveRequestRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledLeaveRequestRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledLeaveRequestRollupAdjuster(List<double> Bounds, List<double> Rates) : IScheduledLeaveRequestRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledLeaveRequestRollup item, double valueValue)
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

/// <summary>Runs the scheduled leave request rollup steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestRollupEngine(List<IScheduledLeaveRequestRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestRollupEngine DefaultScheduledLeaveRequestRollupEngine()
    {
        return new ScheduledLeaveRequestRollupEngine(
            new List<IScheduledLeaveRequestRollupStep>
            {
                new MinimumScheduledLeaveRequestRollupAdjuster(47.1),
                new ScheduledLeaveRequestRollupExpiresAtOffsetAdjuster(0.3),
                new PercentScheduledLeaveRequestRollupAdjuster(16.0),
                new TieredScheduledLeaveRequestRollupAdjuster(
                    new List<double> { 2444.0, 3968.0 },
                    new List<double> { 0.87, 1.14 }),
            },
            new List<string>(),
            4.9,
            42133.4);
    }

    public double Price(ScheduledLeaveRequestRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
