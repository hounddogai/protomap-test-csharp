namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request audit calculation.</summary>
public interface ILeaveRequestAuditAdjuster
{
    string Describe();
    double Adjust(LeaveRequestAudit item, double valueValue);
}

public sealed record PercentLeaveRequestAuditAdjuster(double Percent) : ILeaveRequestAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLeaveRequestAuditAdjuster(double Cap) : ILeaveRequestAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLeaveRequestAuditAdjuster(double Minimum) : ILeaveRequestAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLeaveRequestAuditAdjuster(double Step) : ILeaveRequestAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LeaveRequestAuditFteOffsetAdjuster(double Weight) : ILeaveRequestAuditAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(LeaveRequestAudit item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the leave request audit steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestAuditEngine(List<ILeaveRequestAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestAuditEngine DefaultLeaveRequestAuditEngine()
    {
        return new LeaveRequestAuditEngine(
            new List<ILeaveRequestAuditAdjuster>
            {
                new PercentLeaveRequestAuditAdjuster(-23.8),
                new CapLeaveRequestAuditAdjuster(21310.0),
                new MinimumLeaveRequestAuditAdjuster(27.3),
                new RoundingLeaveRequestAuditAdjuster(0.05),
                new LeaveRequestAuditFteOffsetAdjuster(0.65),
            },
            new List<string>(),
            1.0,
            50335.3);
    }

    public double Evaluate(LeaveRequestAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
