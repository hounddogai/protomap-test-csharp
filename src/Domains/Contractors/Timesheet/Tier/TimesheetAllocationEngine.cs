namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet allocation calculation.</summary>
public interface ITimesheetAllocationModifier
{
    string Describe();
    double Adjust(TimesheetAllocation item, double valueValue);
}

public sealed record PercentTimesheetAllocationAdjuster(double Percent) : ITimesheetAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTimesheetAllocationAdjuster(double Cap) : ITimesheetAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TimesheetAllocationWeightOffsetAdjuster(double Weight) : ITimesheetAllocationModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(TimesheetAllocation item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumTimesheetAllocationAdjuster(double Minimum) : ITimesheetAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTimesheetAllocationAdjuster(double Step) : ITimesheetAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentTimesheetAllocationAdjuster(Dictionary<string, double> Factors) : ITimesheetAllocationModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TimesheetAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the timesheet allocation steps in order and keeps a trace of each.</summary>
public sealed class TimesheetAllocationEngine(List<ITimesheetAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetAllocationEngine DefaultTimesheetAllocationEngine()
    {
        return new TimesheetAllocationEngine(
            new List<ITimesheetAllocationModifier>
            {
                new PercentTimesheetAllocationAdjuster(16.2),
                new CapTimesheetAllocationAdjuster(27222.0),
                new TimesheetAllocationWeightOffsetAdjuster(0.88),
                new MinimumTimesheetAllocationAdjuster(49.8),
                new RoundingTimesheetAllocationAdjuster(0.1),
                new SegmentTimesheetAllocationAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.04, ["retail"] = 0.78 }),
            },
            new List<string>(),
            2.7,
            53570.6);
    }

    public double Compute(TimesheetAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
