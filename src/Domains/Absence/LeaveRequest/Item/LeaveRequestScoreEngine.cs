namespace Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>One step of the leave request score calculation.</summary>
public interface ILeaveRequestScoreStep
{
    string Describe();
    double Adjust(LeaveRequestScore item, double valueValue);
}

public sealed record PercentLeaveRequestScoreAdjuster(double Percent) : ILeaveRequestScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLeaveRequestScoreAdjuster(double Cap) : ILeaveRequestScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLeaveRequestScoreAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestScore item, double valueValue)
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

public sealed record LeaveRequestScoreCostOffsetAdjuster(double Weight) : ILeaveRequestScoreStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LeaveRequestScore item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record SourceLeaveRequestScoreAdjuster(Dictionary<string, double> Factors) : ILeaveRequestScoreStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LeaveRequestScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the leave request score steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestScoreEngine(List<ILeaveRequestScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestScoreEngine DefaultLeaveRequestScoreEngine()
    {
        return new LeaveRequestScoreEngine(
            new List<ILeaveRequestScoreStep>
            {
                new PercentLeaveRequestScoreAdjuster(23.7),
                new CapLeaveRequestScoreAdjuster(3735.0),
                new TieredLeaveRequestScoreAdjuster(
                    new List<double> { 4372.0, 5581.0, 6033.0, 7774.0 },
                    new List<double> { 1.09, 1.19, 1.14, 1.08 }),
                new LeaveRequestScoreCostOffsetAdjuster(0.22),
                new SourceLeaveRequestScoreAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.77, ["south"] = 1.03 }),
            },
            new List<string>(),
            4.0,
            86959.8);
    }

    public double Evaluate(LeaveRequestScore item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
