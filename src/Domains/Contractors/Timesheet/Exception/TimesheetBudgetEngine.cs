namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet budget calculation.</summary>
public interface ITimesheetBudgetModifier
{
    string Describe();
    double Adjust(TimesheetBudget item, double valueValue);
}

public sealed record CapTimesheetBudgetAdjuster(double Cap) : ITimesheetBudgetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTimesheetBudgetAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetBudget item, double valueValue)
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

public sealed record FlaggedTimesheetBudgetAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTimesheetBudgetAdjuster(double Step) : ITimesheetBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTimesheetBudgetAdjuster(double Percent) : ITimesheetBudgetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet budget steps in order and keeps a trace of each.</summary>
public sealed class TimesheetBudgetEngine(List<ITimesheetBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetBudgetEngine DefaultTimesheetBudgetEngine(Registry flags)
    {
        return new TimesheetBudgetEngine(
            new List<ITimesheetBudgetModifier>
            {
                new CapTimesheetBudgetAdjuster(5153.0),
                new TieredTimesheetBudgetAdjuster(
                    new List<double> { 5829.0, 9014.0 },
                    new List<double> { 1.09, 0.82 }),
                new FlaggedTimesheetBudgetAdjuster(flags, "timesheet-budget-scheduled", -6.3),
                new RoundingTimesheetBudgetAdjuster(5.0),
                new PercentTimesheetBudgetAdjuster(-6.1),
            },
            new List<string>(),
            4.7,
            5612.8);
    }

    public double Evaluate(TimesheetBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
