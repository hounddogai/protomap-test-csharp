namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped timesheet plan calculation.</summary>
public interface ICappedTimesheetPlanAdjuster
{
    string Describe();
    double Adjust(CappedTimesheetPlan item, double valueValue);
}

public sealed record CappedTimesheetPlanMinimumOffsetAdjuster(double Weight) : ICappedTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(CappedTimesheetPlan item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CapCappedTimesheetPlanAdjuster(double Cap) : ICappedTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedTimesheetPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedTimesheetPlanAdjuster(double Step) : ICappedTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedTimesheetPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameCappedTimesheetPlanAdjuster(Dictionary<string, double> Factors) : ICappedTimesheetPlanAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CappedTimesheetPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCappedTimesheetPlanAdjuster(Registry Flags, string Flag, double Percent) : ICappedTimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedTimesheetPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped timesheet plan steps in order and keeps a trace of each.</summary>
public sealed class CappedTimesheetPlanEngine(List<ICappedTimesheetPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedTimesheetPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedTimesheetPlanEngine DefaultCappedTimesheetPlanEngine(Registry flags)
    {
        return new CappedTimesheetPlanEngine(
            new List<ICappedTimesheetPlanAdjuster>
            {
                new CappedTimesheetPlanMinimumOffsetAdjuster(1.13),
                new CapCappedTimesheetPlanAdjuster(8740.0),
                new RoundingCappedTimesheetPlanAdjuster(0.1),
                new NameCappedTimesheetPlanAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.37, ["south"] = 1.15 }),
                new FlaggedCappedTimesheetPlanAdjuster(flags, "capped-timesheet-plan-seasonal", -5.2),
            },
            new List<string>(),
            1.9,
            88670.0);
    }

    public double Price(CappedTimesheetPlan item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedTimesheetPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
