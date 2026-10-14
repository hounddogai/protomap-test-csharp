namespace Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>One step of the regional timesheet window calculation.</summary>
public interface IRegionalTimesheetWindowFactor
{
    string Describe();
    double Adjust(RegionalTimesheetWindow item, double valueValue);
}

public sealed record MinimumRegionalTimesheetWindowAdjuster(double Minimum) : IRegionalTimesheetWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalTimesheetWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalTimesheetWindowAdjuster(double Cap) : IRegionalTimesheetWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalTimesheetWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleRegionalTimesheetWindowAdjuster(Dictionary<string, double> Factors) : IRegionalTimesheetWindowFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RegionalTimesheetWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalTimesheetWindowMinimumOffsetAdjuster(double Weight) : IRegionalTimesheetWindowFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(RegionalTimesheetWindow item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the regional timesheet window steps in order and keeps a trace of each.</summary>
public sealed class RegionalTimesheetWindowEngine(List<IRegionalTimesheetWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTimesheetWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTimesheetWindowEngine DefaultRegionalTimesheetWindowEngine()
    {
        return new RegionalTimesheetWindowEngine(
            new List<IRegionalTimesheetWindowFactor>
            {
                new MinimumRegionalTimesheetWindowAdjuster(15.5),
                new CapRegionalTimesheetWindowAdjuster(17503.0),
                new TitleRegionalTimesheetWindowAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.98, ["west"] = 1.1, ["online"] = 0.78 }),
                new RegionalTimesheetWindowMinimumOffsetAdjuster(1.05),
            },
            new List<string>(),
            4.6,
            88812.2);
    }

    public double Run(RegionalTimesheetWindow item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTimesheetWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
