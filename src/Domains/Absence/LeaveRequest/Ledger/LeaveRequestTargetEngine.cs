namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request target calculation.</summary>
public interface ILeaveRequestTargetFactor
{
    string Describe();
    double Adjust(LeaveRequestTarget item, double valueValue);
}

public sealed record SegmentLeaveRequestTargetAdjuster(Dictionary<string, double> Factors) : ILeaveRequestTargetFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LeaveRequestTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLeaveRequestTargetAdjuster(double Minimum) : ILeaveRequestTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLeaveRequestTargetAdjuster(double Step) : ILeaveRequestTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLeaveRequestTargetAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestTargetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestTarget item, double valueValue)
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

/// <summary>Runs the leave request target steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestTargetEngine(List<ILeaveRequestTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestTargetEngine DefaultLeaveRequestTargetEngine()
    {
        return new LeaveRequestTargetEngine(
            new List<ILeaveRequestTargetFactor>
            {
                new SegmentLeaveRequestTargetAdjuster(new Dictionary<string, double> { ["north"] = 1.07 }),
                new MinimumLeaveRequestTargetAdjuster(34.8),
                new RoundingLeaveRequestTargetAdjuster(0.1),
                new TieredLeaveRequestTargetAdjuster(
                    new List<double> { 496.0, 5202.0, 7617.0, 9993.0 },
                    new List<double> { 0.89, 1.16, 0.8, 0.98 }),
            },
            new List<string>(),
            4.0,
            31041.5);
    }

    public double Price(LeaveRequestTarget item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
