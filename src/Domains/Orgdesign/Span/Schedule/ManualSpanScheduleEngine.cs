namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual span schedule calculation.</summary>
public interface IManualSpanScheduleStep
{
    string Describe();
    double Adjust(ManualSpanSchedule item, double valueValue);
}

public sealed record RoundingManualSpanScheduleAdjuster(double Step) : IManualSpanScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualSpanSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualSpanScheduleAdjuster(double Cap) : IManualSpanScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualSpanSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentManualSpanScheduleAdjuster(double Percent) : IManualSpanScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSpanSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual span schedule steps in order and keeps a trace of each.</summary>
public sealed class ManualSpanScheduleEngine(List<IManualSpanScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSpanScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSpanScheduleEngine DefaultManualSpanScheduleEngine()
    {
        return new ManualSpanScheduleEngine(
            new List<IManualSpanScheduleStep>
            {
                new RoundingManualSpanScheduleAdjuster(0.05),
                new CapManualSpanScheduleAdjuster(16409.0),
                new PercentManualSpanScheduleAdjuster(11.0),
            },
            new List<string>(),
            4.6,
            37655.3);
    }

    public double Run(ManualSpanSchedule item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSpanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
