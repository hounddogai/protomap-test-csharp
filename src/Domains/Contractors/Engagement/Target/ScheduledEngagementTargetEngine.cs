namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled engagement target calculation.</summary>
public interface IScheduledEngagementTargetAdjuster
{
    string Describe();
    double Adjust(ScheduledEngagementTarget item, double valueValue);
}

public sealed record CapScheduledEngagementTargetAdjuster(double Cap) : IScheduledEngagementTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledEngagementTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledEngagementTargetAdjuster(double Percent) : IScheduledEngagementTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledEngagementTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledEngagementTargetFteOffsetAdjuster(double Weight) : IScheduledEngagementTargetAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(ScheduledEngagementTarget item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

/// <summary>Runs the scheduled engagement target steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEngagementTargetEngine(List<IScheduledEngagementTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEngagementTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEngagementTargetEngine DefaultScheduledEngagementTargetEngine()
    {
        return new ScheduledEngagementTargetEngine(
            new List<IScheduledEngagementTargetAdjuster>
            {
                new CapScheduledEngagementTargetAdjuster(4319.0),
                new PercentScheduledEngagementTargetAdjuster(11.1),
                new ScheduledEngagementTargetFteOffsetAdjuster(0.92),
            },
            new List<string>(),
            4.2,
            77185.0);
    }

    public double Evaluate(ScheduledEngagementTarget item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEngagementTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
