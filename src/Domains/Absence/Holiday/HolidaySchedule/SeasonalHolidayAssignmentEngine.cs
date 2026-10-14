namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal holiday assignment calculation.</summary>
public interface ISeasonalHolidayAssignmentStep
{
    string Describe();
    double Adjust(SeasonalHolidayAssignment item, double valueValue);
}

public sealed record FlaggedSeasonalHolidayAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalHolidayAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalHolidayAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalHolidayAssignmentAdjuster(double Minimum) : ISeasonalHolidayAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalHolidayAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalHolidayAssignmentAdjuster(double Cap) : ISeasonalHolidayAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalHolidayAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal holiday assignment steps in order and keeps a trace of each.</summary>
public sealed class SeasonalHolidayAssignmentEngine(List<ISeasonalHolidayAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalHolidayAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalHolidayAssignmentEngine DefaultSeasonalHolidayAssignmentEngine(Registry flags)
    {
        return new SeasonalHolidayAssignmentEngine(
            new List<ISeasonalHolidayAssignmentStep>
            {
                new FlaggedSeasonalHolidayAssignmentAdjuster(flags, "seasonal-holiday-assignment-fallback", 3.0),
                new MinimumSeasonalHolidayAssignmentAdjuster(17.0),
                new CapSeasonalHolidayAssignmentAdjuster(15306.0),
            },
            new List<string>(),
            0.3,
            34160.5);
    }

    public double Evaluate(SeasonalHolidayAssignment item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalHolidayAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
