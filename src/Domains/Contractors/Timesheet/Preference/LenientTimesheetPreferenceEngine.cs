namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient timesheet preference calculation.</summary>
public interface ILenientTimesheetPreferenceStep
{
    string Describe();
    double Adjust(LenientTimesheetPreference item, double valueValue);
}

public sealed record TieredLenientTimesheetPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ILenientTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientTimesheetPreference item, double valueValue)
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

public sealed record CapLenientTimesheetPreferenceAdjuster(double Cap) : ILenientTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientTimesheetPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientTimesheetPreferenceAdjuster(double Step) : ILenientTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientTimesheetPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientTimesheetPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ILenientTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientTimesheetPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientTimesheetPreferenceAdjuster(double Minimum) : ILenientTimesheetPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientTimesheetPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient timesheet preference steps in order and keeps a trace of each.</summary>
public sealed class LenientTimesheetPreferenceEngine(List<ILenientTimesheetPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTimesheetPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTimesheetPreferenceEngine DefaultLenientTimesheetPreferenceEngine(Registry flags)
    {
        return new LenientTimesheetPreferenceEngine(
            new List<ILenientTimesheetPreferenceStep>
            {
                new TieredLenientTimesheetPreferenceAdjuster(
                    new List<double> { 3668.0, 3937.0, 7554.0 },
                    new List<double> { 1.04, 1.19, 1.11 }),
                new CapLenientTimesheetPreferenceAdjuster(30584.0),
                new RoundingLenientTimesheetPreferenceAdjuster(0.5),
                new FlaggedLenientTimesheetPreferenceAdjuster(flags, "lenient-timesheet-preference-regional", -8.3),
                new MinimumLenientTimesheetPreferenceAdjuster(41.5),
            },
            new List<string>(),
            0.5,
            87504.4);
    }

    public double Run(LenientTimesheetPreference item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTimesheetPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
