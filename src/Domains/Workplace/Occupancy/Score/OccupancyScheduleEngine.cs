namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy schedule calculation.</summary>
public interface IOccupancyScheduleStep
{
    string Describe();
    double Adjust(OccupancySchedule item, double valueValue);
}

public sealed record DepartmentOccupancyScheduleAdjuster(Dictionary<string, double> Factors) : IOccupancyScheduleStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(OccupancySchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentOccupancyScheduleAdjuster(double Percent) : IOccupancyScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancySchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOccupancyScheduleAdjuster(double Cap) : IOccupancyScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OccupancySchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingOccupancyScheduleAdjuster(double Step) : IOccupancyScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancySchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the occupancy schedule steps in order and keeps a trace of each.</summary>
public sealed class OccupancyScheduleEngine(List<IOccupancyScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyScheduleEngine DefaultOccupancyScheduleEngine()
    {
        return new OccupancyScheduleEngine(
            new List<IOccupancyScheduleStep>
            {
                new DepartmentOccupancyScheduleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.07, ["north"] = 1.22 }),
                new PercentOccupancyScheduleAdjuster(4.5),
                new CapOccupancyScheduleAdjuster(40505.0),
                new RoundingOccupancyScheduleAdjuster(0.05),
            },
            new List<string>(),
            2.8,
            47567.2);
    }

    public double Price(OccupancySchedule item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancySchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
