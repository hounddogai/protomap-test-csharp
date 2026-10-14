namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled statement of work window calculation.</summary>
public interface IScheduledStatementOfWorkWindowStep
{
    string Describe();
    double Adjust(ScheduledStatementOfWorkWindow item, double valueValue);
}

public sealed record CountryScheduledStatementOfWorkWindowAdjuster(Dictionary<string, double> Factors) : IScheduledStatementOfWorkWindowStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScheduledStatementOfWorkWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledStatementOfWorkWindowAdjuster(double Cap) : IScheduledStatementOfWorkWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledStatementOfWorkWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledStatementOfWorkWindowAdjuster(double Percent) : IScheduledStatementOfWorkWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledStatementOfWorkWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled statement of work window steps in order and keeps a trace of each.</summary>
public sealed class ScheduledStatementOfWorkWindowEngine(List<IScheduledStatementOfWorkWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledStatementOfWorkWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledStatementOfWorkWindowEngine DefaultScheduledStatementOfWorkWindowEngine()
    {
        return new ScheduledStatementOfWorkWindowEngine(
            new List<IScheduledStatementOfWorkWindowStep>
            {
                new CountryScheduledStatementOfWorkWindowAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.91 }),
                new CapScheduledStatementOfWorkWindowAdjuster(3096.0),
                new PercentScheduledStatementOfWorkWindowAdjuster(-19.9),
            },
            new List<string>(),
            2.4,
            78733.4);
    }

    public double Compute(ScheduledStatementOfWorkWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledStatementOfWorkWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
