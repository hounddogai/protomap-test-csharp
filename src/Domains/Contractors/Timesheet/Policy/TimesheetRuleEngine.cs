namespace Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>One step of the timesheet rule calculation.</summary>
public interface ITimesheetRuleModifier
{
    string Describe();
    double Adjust(TimesheetRule item, double valueValue);
}

public sealed record TimesheetRuleStartsAtOffsetAdjuster(double Weight) : ITimesheetRuleModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(TimesheetRule item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record TitleTimesheetRuleAdjuster(Dictionary<string, double> Factors) : ITimesheetRuleModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TimesheetRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTimesheetRuleAdjuster(double Minimum) : ITimesheetRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the timesheet rule steps in order and keeps a trace of each.</summary>
public sealed class TimesheetRuleEngine(List<ITimesheetRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetRuleEngine DefaultTimesheetRuleEngine()
    {
        return new TimesheetRuleEngine(
            new List<ITimesheetRuleModifier>
            {
                new TimesheetRuleStartsAtOffsetAdjuster(0.68),
                new TitleTimesheetRuleAdjuster(new Dictionary<string, double> { ["east"] = 0.86 }),
                new MinimumTimesheetRuleAdjuster(7.8),
            },
            new List<string>(),
            0.3,
            77664.7);
    }

    public double Price(TimesheetRule item)
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
