namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition schedule calculation.</summary>
public interface IRecognitionScheduleStep
{
    string Describe();
    double Adjust(RecognitionSchedule item, double valueValue);
}

public sealed record CapRecognitionScheduleAdjuster(double Cap) : IRecognitionScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRecognitionScheduleAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRecognitionScheduleAdjuster(double Percent) : IRecognitionScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the recognition schedule steps in order and keeps a trace of each.</summary>
public sealed class RecognitionScheduleEngine(List<IRecognitionScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionScheduleEngine DefaultRecognitionScheduleEngine(Registry flags)
    {
        return new RecognitionScheduleEngine(
            new List<IRecognitionScheduleStep>
            {
                new CapRecognitionScheduleAdjuster(31975.0),
                new FlaggedRecognitionScheduleAdjuster(flags, "recognition-schedule-tiered", 9.0),
                new PercentRecognitionScheduleAdjuster(23.9),
            },
            new List<string>(),
            4.1,
            66284.1);
    }

    public double Run(RecognitionSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
