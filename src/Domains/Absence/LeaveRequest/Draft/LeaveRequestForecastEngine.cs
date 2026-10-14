namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request forecast calculation.</summary>
public interface ILeaveRequestForecastStep
{
    string Describe();
    double Adjust(LeaveRequestForecast item, double valueValue);
}

public sealed record PercentLeaveRequestForecastAdjuster(double Percent) : ILeaveRequestForecastStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LeaveRequestForecastRetriesOffsetAdjuster(double Weight) : ILeaveRequestForecastStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(LeaveRequestForecast item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record RoundingLeaveRequestForecastAdjuster(double Step) : ILeaveRequestForecastStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLeaveRequestForecastAdjuster(double Cap) : ILeaveRequestForecastStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the leave request forecast steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestForecastEngine(List<ILeaveRequestForecastStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestForecastStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestForecastEngine DefaultLeaveRequestForecastEngine()
    {
        return new LeaveRequestForecastEngine(
            new List<ILeaveRequestForecastStep>
            {
                new PercentLeaveRequestForecastAdjuster(-7.8),
                new LeaveRequestForecastRetriesOffsetAdjuster(1.5),
                new RoundingLeaveRequestForecastAdjuster(1.0),
                new CapLeaveRequestForecastAdjuster(39658.0),
            },
            new List<string>(),
            2.6,
            95232.8);
    }

    public double Evaluate(LeaveRequestForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
