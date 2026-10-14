namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual assignment schedule calculation.</summary>
public interface IManualAssignmentScheduleStep
{
    string Describe();
    double Adjust(ManualAssignmentSchedule item, double valueValue);
}

public sealed record ManualAssignmentScheduleDurationMinutesOffsetAdjuster(double Weight) : IManualAssignmentScheduleStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(ManualAssignmentSchedule item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record FlaggedManualAssignmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : IManualAssignmentScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAssignmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualAssignmentScheduleAdjuster(double Cap) : IManualAssignmentScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAssignmentSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual assignment schedule steps in order and keeps a trace of each.</summary>
public sealed class ManualAssignmentScheduleEngine(List<IManualAssignmentScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAssignmentScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAssignmentScheduleEngine DefaultManualAssignmentScheduleEngine(Registry flags)
    {
        return new ManualAssignmentScheduleEngine(
            new List<IManualAssignmentScheduleStep>
            {
                new ManualAssignmentScheduleDurationMinutesOffsetAdjuster(0.37),
                new FlaggedManualAssignmentScheduleAdjuster(flags, "manual-assignment-schedule-regional", 5.8),
                new CapManualAssignmentScheduleAdjuster(32962.0),
            },
            new List<string>(),
            0.6,
            14301.4);
    }

    public double Price(ManualAssignmentSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualAssignmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
