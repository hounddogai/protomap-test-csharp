namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>One step of the leave request baseline calculation.</summary>
public interface ILeaveRequestBaselineStep
{
    string Describe();
    double Adjust(LeaveRequestBaseline item, double valueValue);
}

public sealed record TitleLeaveRequestBaselineAdjuster(Dictionary<string, double> Factors) : ILeaveRequestBaselineStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LeaveRequestBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLeaveRequestBaselineAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestBaseline item, double valueValue)
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

public sealed record CapLeaveRequestBaselineAdjuster(double Cap) : ILeaveRequestBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the leave request baseline steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestBaselineEngine(List<ILeaveRequestBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestBaselineEngine DefaultLeaveRequestBaselineEngine()
    {
        return new LeaveRequestBaselineEngine(
            new List<ILeaveRequestBaselineStep>
            {
                new TitleLeaveRequestBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.73, ["south"] = 1.05, ["central"] = 1.02 }),
                new TieredLeaveRequestBaselineAdjuster(
                    new List<double> { 166.0, 1301.0 },
                    new List<double> { 1.16, 1.13 }),
                new CapLeaveRequestBaselineAdjuster(2312.0),
            },
            new List<string>(),
            2.6,
            18998.5);
    }

    public double Price(LeaveRequestBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
