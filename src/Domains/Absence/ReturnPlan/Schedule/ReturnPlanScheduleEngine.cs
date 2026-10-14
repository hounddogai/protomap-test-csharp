namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan schedule calculation.</summary>
public interface IReturnPlanScheduleStep
{
    string Describe();
    double Adjust(ReturnPlanSchedule item, double valueValue);
}

public sealed record FlaggedReturnPlanScheduleAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReturnPlanScheduleAdjuster(double Cap) : IReturnPlanScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingReturnPlanScheduleAdjuster(double Step) : IReturnPlanScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the return plan schedule steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanScheduleEngine(List<IReturnPlanScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanScheduleEngine DefaultReturnPlanScheduleEngine(Registry flags)
    {
        return new ReturnPlanScheduleEngine(
            new List<IReturnPlanScheduleStep>
            {
                new FlaggedReturnPlanScheduleAdjuster(flags, "return-plan-schedule-adaptive", -4.2),
                new CapReturnPlanScheduleAdjuster(45574.0),
                new RoundingReturnPlanScheduleAdjuster(0.05),
            },
            new List<string>(),
            0.4,
            18303.4);
    }

    public double Evaluate(ReturnPlanSchedule item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
