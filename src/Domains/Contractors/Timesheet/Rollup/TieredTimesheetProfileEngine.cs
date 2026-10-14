namespace Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>One step of the tiered timesheet profile calculation.</summary>
public interface ITieredTimesheetProfileAdjuster
{
    string Describe();
    double Adjust(TieredTimesheetProfile item, double valueValue);
}

public sealed record CapTieredTimesheetProfileAdjuster(double Cap) : ITieredTimesheetProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredTimesheetProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTieredTimesheetProfileAdjuster(double Percent) : ITieredTimesheetProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredTimesheetProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredTimesheetProfileAdjuster(double Minimum) : ITieredTimesheetProfileAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredTimesheetProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamTieredTimesheetProfileAdjuster(Dictionary<string, double> Factors) : ITieredTimesheetProfileAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(TieredTimesheetProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered timesheet profile steps in order and keeps a trace of each.</summary>
public sealed class TieredTimesheetProfileEngine(List<ITieredTimesheetProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredTimesheetProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredTimesheetProfileEngine DefaultTieredTimesheetProfileEngine()
    {
        return new TieredTimesheetProfileEngine(
            new List<ITieredTimesheetProfileAdjuster>
            {
                new CapTieredTimesheetProfileAdjuster(5219.0),
                new PercentTieredTimesheetProfileAdjuster(10.4),
                new MinimumTieredTimesheetProfileAdjuster(46.3),
                new TeamTieredTimesheetProfileAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["central"] = 1.38 }),
            },
            new List<string>(),
            3.1,
            3689.6);
    }

    public double Run(TieredTimesheetProfile item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredTimesheetProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
