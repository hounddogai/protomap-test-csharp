namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request allocation calculation.</summary>
public interface ILeaveRequestAllocationFactor
{
    string Describe();
    double Adjust(LeaveRequestAllocation item, double valueValue);
}

public sealed record LeaveRequestAllocationExpiresAtOffsetAdjuster(double Weight) : ILeaveRequestAllocationFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LeaveRequestAllocation item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record RoundingLeaveRequestAllocationAdjuster(double Step) : ILeaveRequestAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLeaveRequestAllocationAdjuster(double Cap) : ILeaveRequestAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLeaveRequestAllocationAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestAllocation item, double valueValue)
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

public sealed record MinimumLeaveRequestAllocationAdjuster(double Minimum) : ILeaveRequestAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the leave request allocation steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestAllocationEngine(List<ILeaveRequestAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestAllocationEngine DefaultLeaveRequestAllocationEngine()
    {
        return new LeaveRequestAllocationEngine(
            new List<ILeaveRequestAllocationFactor>
            {
                new LeaveRequestAllocationExpiresAtOffsetAdjuster(1.93),
                new RoundingLeaveRequestAllocationAdjuster(0.1),
                new CapLeaveRequestAllocationAdjuster(36466.0),
                new TieredLeaveRequestAllocationAdjuster(
                    new List<double> { 5572.0, 7987.0 },
                    new List<double> { 0.95, 1.07 }),
                new MinimumLeaveRequestAllocationAdjuster(22.2),
            },
            new List<string>(),
            2.2,
            33993.6);
    }

    public double Compute(LeaveRequestAllocation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
