namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the assignment schedule calculation.</summary>
public interface IAssignmentScheduleStep
{
    string Describe();
    double Adjust(AssignmentSchedule item, double valueValue);
}

public sealed record CapAssignmentScheduleAdjuster(double Cap) : IAssignmentScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAssignmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssignmentScheduleAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentScheduleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentSchedule item, double valueValue)
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

public sealed record UnitAssignmentScheduleAdjuster(Dictionary<string, double> Factors) : IAssignmentScheduleStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AssignmentSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the assignment schedule steps in order and keeps a trace of each.</summary>
public sealed class AssignmentScheduleEngine(List<IAssignmentScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentScheduleEngine DefaultAssignmentScheduleEngine(Registry flags)
    {
        return new AssignmentScheduleEngine(
            new List<IAssignmentScheduleStep>
            {
                new CapAssignmentScheduleAdjuster(33508.0),
                new FlaggedAssignmentScheduleAdjuster(flags, "assignment-schedule-strict", 1.3),
                new TieredAssignmentScheduleAdjuster(
                    new List<double> { 7094.0, 7485.0, 9753.0 },
                    new List<double> { 0.93, 1.0, 1.07 }),
                new UnitAssignmentScheduleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.05, ["north"] = 0.97, ["retail"] = 1.39 }),
            },
            new List<string>(),
            3.2,
            82269.7);
    }

    public double Compute(AssignmentSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
