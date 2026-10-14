namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default timesheet preference calculation.</summary>
public interface IDefaultTimesheetPreferenceStep
{
    string Describe();
    double Adjust(DefaultTimesheetPreference item, double valueValue);
}

public sealed record MinimumDefaultTimesheetPreferenceAdjuster(double Minimum) : IDefaultTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultTimesheetPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultTimesheetPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IDefaultTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultTimesheetPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultTimesheetPreferenceAdjuster(double Step) : IDefaultTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultTimesheetPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultTimesheetPreferenceAdjuster(double Percent) : IDefaultTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultTimesheetPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultTimesheetPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IDefaultTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultTimesheetPreference item, double valueValue)
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

public sealed record CapDefaultTimesheetPreferenceAdjuster(double Cap) : IDefaultTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultTimesheetPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default timesheet preference steps in order and keeps a trace of each.</summary>
public sealed class DefaultTimesheetPreferenceEngine(List<IDefaultTimesheetPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultTimesheetPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultTimesheetPreferenceEngine DefaultDefaultTimesheetPreferenceEngine(Registry flags)
    {
        return new DefaultTimesheetPreferenceEngine(
            new List<IDefaultTimesheetPreferenceStep>
            {
                new MinimumDefaultTimesheetPreferenceAdjuster(4.9),
                new FlaggedDefaultTimesheetPreferenceAdjuster(flags, "default-timesheet-preference-manual", 5.2),
                new RoundingDefaultTimesheetPreferenceAdjuster(1.0),
                new PercentDefaultTimesheetPreferenceAdjuster(17.1),
                new TieredDefaultTimesheetPreferenceAdjuster(
                    new List<double> { 359.0, 8645.0 },
                    new List<double> { 0.89, 1.15 }),
                new CapDefaultTimesheetPreferenceAdjuster(37767.0),
            },
            new List<string>(),
            2.0,
            20181.3);
    }

    public double Run(DefaultTimesheetPreference item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultTimesheetPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
