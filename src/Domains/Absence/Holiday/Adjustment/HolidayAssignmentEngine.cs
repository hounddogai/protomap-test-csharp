namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday assignment calculation.</summary>
public interface IHolidayAssignmentStep
{
    string Describe();
    double Adjust(HolidayAssignment item, double valueValue);
}

public sealed record HolidayAssignmentBudgetOffsetAdjuster(double Weight) : IHolidayAssignmentStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(HolidayAssignment item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapHolidayAssignmentAdjuster(double Cap) : IHolidayAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedHolidayAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IHolidayAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingHolidayAssignmentAdjuster(double Step) : IHolidayAssignmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamHolidayAssignmentAdjuster(Dictionary<string, double> Factors) : IHolidayAssignmentStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(HolidayAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the holiday assignment steps in order and keeps a trace of each.</summary>
public sealed class HolidayAssignmentEngine(List<IHolidayAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayAssignmentEngine DefaultHolidayAssignmentEngine(Registry flags)
    {
        return new HolidayAssignmentEngine(
            new List<IHolidayAssignmentStep>
            {
                new HolidayAssignmentBudgetOffsetAdjuster(1.62),
                new CapHolidayAssignmentAdjuster(29735.0),
                new FlaggedHolidayAssignmentAdjuster(flags, "holiday-assignment-scheduled", -5.9),
                new RoundingHolidayAssignmentAdjuster(5.0),
                new TeamHolidayAssignmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.91, ["east"] = 0.71 }),
            },
            new List<string>(),
            3.8,
            75874.6);
    }

    public double Price(HolidayAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<HolidayAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
