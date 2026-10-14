namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

/// <summary>One step of the leave request record calculation.</summary>
public interface ILeaveRequestRecordModifier
{
    string Describe();
    double Adjust(LeaveRequestRecord item, double valueValue);
}

public sealed record MinimumLeaveRequestRecordAdjuster(double Minimum) : ILeaveRequestRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLeaveRequestRecordAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestRecord item, double valueValue)
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

public sealed record PercentLeaveRequestRecordAdjuster(double Percent) : ILeaveRequestRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request record steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestRecordEngine(List<ILeaveRequestRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestRecordEngine DefaultLeaveRequestRecordEngine()
    {
        return new LeaveRequestRecordEngine(
            new List<ILeaveRequestRecordModifier>
            {
                new MinimumLeaveRequestRecordAdjuster(10.9),
                new TieredLeaveRequestRecordAdjuster(
                    new List<double> { 2019.0, 3699.0, 4541.0, 6378.0 },
                    new List<double> { 1.04, 0.99, 0.82, 0.94 }),
                new PercentLeaveRequestRecordAdjuster(8.5),
            },
            new List<string>(),
            4.2,
            94619.0);
    }

    public double Price(LeaveRequestRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
