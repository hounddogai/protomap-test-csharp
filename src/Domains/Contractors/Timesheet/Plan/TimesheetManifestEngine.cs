namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet manifest calculation.</summary>
public interface ITimesheetManifestModifier
{
    string Describe();
    double Adjust(TimesheetManifest item, double valueValue);
}

public sealed record CategoryTimesheetManifestAdjuster(Dictionary<string, double> Factors) : ITimesheetManifestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(TimesheetManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTimesheetManifestAdjuster(double Percent) : ITimesheetManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTimesheetManifestAdjuster(double Step) : ITimesheetManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTimesheetManifestAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetManifest item, double valueValue)
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

public sealed record MinimumTimesheetManifestAdjuster(double Minimum) : ITimesheetManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the timesheet manifest steps in order and keeps a trace of each.</summary>
public sealed class TimesheetManifestEngine(List<ITimesheetManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetManifestEngine DefaultTimesheetManifestEngine()
    {
        return new TimesheetManifestEngine(
            new List<ITimesheetManifestModifier>
            {
                new CategoryTimesheetManifestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.2, ["south"] = 1.06, ["online"] = 1.27 }),
                new PercentTimesheetManifestAdjuster(-2.2),
                new RoundingTimesheetManifestAdjuster(0.25),
                new TieredTimesheetManifestAdjuster(
                    new List<double> { 1263.0, 2439.0 },
                    new List<double> { 1.13, 1.15 }),
                new MinimumTimesheetManifestAdjuster(48.5),
            },
            new List<string>(),
            4.7,
            48521.6);
    }

    public double Price(TimesheetManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
