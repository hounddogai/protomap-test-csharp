namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request batch calculation.</summary>
public interface ILeaveRequestBatchStep
{
    string Describe();
    double Adjust(LeaveRequestBatch item, double valueValue);
}

public sealed record MinimumLeaveRequestBatchAdjuster(double Minimum) : ILeaveRequestBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLeaveRequestBatchAdjuster(double Percent) : ILeaveRequestBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceLeaveRequestBatchAdjuster(Dictionary<string, double> Factors) : ILeaveRequestBatchStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LeaveRequestBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLeaveRequestBatchAdjuster(double Cap) : ILeaveRequestBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLeaveRequestBatchAdjuster(double Step) : ILeaveRequestBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the leave request batch steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestBatchEngine(List<ILeaveRequestBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestBatchEngine DefaultLeaveRequestBatchEngine()
    {
        return new LeaveRequestBatchEngine(
            new List<ILeaveRequestBatchStep>
            {
                new MinimumLeaveRequestBatchAdjuster(11.5),
                new PercentLeaveRequestBatchAdjuster(-14.1),
                new SourceLeaveRequestBatchAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.16, ["online"] = 1.07 }),
                new CapLeaveRequestBatchAdjuster(23847.0),
                new RoundingLeaveRequestBatchAdjuster(0.05),
            },
            new List<string>(),
            4.1,
            72091.9);
    }

    public double Evaluate(LeaveRequestBatch item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
