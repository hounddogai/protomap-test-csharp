namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request template calculation.</summary>
public interface ILeaveRequestTemplateModifier
{
    string Describe();
    double Adjust(LeaveRequestTemplate item, double valueValue);
}

public sealed record NameLeaveRequestTemplateAdjuster(Dictionary<string, double> Factors) : ILeaveRequestTemplateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LeaveRequestTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LeaveRequestTemplateEndsAtOffsetAdjuster(double Weight) : ILeaveRequestTemplateModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(LeaveRequestTemplate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CapLeaveRequestTemplateAdjuster(double Cap) : ILeaveRequestTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the leave request template steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestTemplateEngine(List<ILeaveRequestTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestTemplateEngine DefaultLeaveRequestTemplateEngine()
    {
        return new LeaveRequestTemplateEngine(
            new List<ILeaveRequestTemplateModifier>
            {
                new NameLeaveRequestTemplateAdjuster(new Dictionary<string, double> { ["online"] = 1.01 }),
                new LeaveRequestTemplateEndsAtOffsetAdjuster(0.41),
                new CapLeaveRequestTemplateAdjuster(11201.0),
            },
            new List<string>(),
            3.9,
            85684.9);
    }

    public double Run(LeaveRequestTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
