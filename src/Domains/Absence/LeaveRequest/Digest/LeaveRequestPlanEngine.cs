namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request plan calculation.</summary>
public interface ILeaveRequestPlanFactor
{
    string Describe();
    double Adjust(LeaveRequestPlan item, double valueValue);
}

public sealed record LeaveRequestPlanCapacityOffsetAdjuster(double Weight) : ILeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(LeaveRequestPlan item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record TieredLeaveRequestPlanAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestPlan item, double valueValue)
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

public sealed record RoundingLeaveRequestPlanAdjuster(double Step) : ILeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameLeaveRequestPlanAdjuster(Dictionary<string, double> Factors) : ILeaveRequestPlanFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LeaveRequestPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLeaveRequestPlanAdjuster(double Cap) : ILeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLeaveRequestPlanAdjuster(double Minimum) : ILeaveRequestPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the leave request plan steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestPlanEngine(List<ILeaveRequestPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestPlanEngine DefaultLeaveRequestPlanEngine()
    {
        return new LeaveRequestPlanEngine(
            new List<ILeaveRequestPlanFactor>
            {
                new LeaveRequestPlanCapacityOffsetAdjuster(0.78),
                new TieredLeaveRequestPlanAdjuster(
                    new List<double> { 1458.0, 5104.0 },
                    new List<double> { 0.88, 1.09 }),
                new RoundingLeaveRequestPlanAdjuster(0.1),
                new NameLeaveRequestPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.73, ["central"] = 0.93 }),
                new CapLeaveRequestPlanAdjuster(882.0),
                new MinimumLeaveRequestPlanAdjuster(17.9),
            },
            new List<string>(),
            2.9,
            17227.6);
    }

    public double Price(LeaveRequestPlan item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
