namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional timesheet profile calculation.</summary>
public interface IRegionalTimesheetProfileModifier
{
    string Describe();
    double Adjust(RegionalTimesheetProfile item, double valueValue);
}

public sealed record RoundingRegionalTimesheetProfileAdjuster(double Step) : IRegionalTimesheetProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalTimesheetProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalTimesheetProfileAdjuster(double Percent) : IRegionalTimesheetProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTimesheetProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalTimesheetProfileAdjuster(double Cap) : IRegionalTimesheetProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalTimesheetProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRegionalTimesheetProfileAdjuster(double Minimum) : IRegionalTimesheetProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalTimesheetProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalTimesheetProfileAdjuster(Registry Flags, string Flag, double Percent) : IRegionalTimesheetProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalTimesheetProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalTimesheetProfileAdjuster(List<double> Bounds, List<double> Rates) : IRegionalTimesheetProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalTimesheetProfile item, double valueValue)
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

/// <summary>Runs the regional timesheet profile steps in order and keeps a trace of each.</summary>
public sealed class RegionalTimesheetProfileEngine(List<IRegionalTimesheetProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTimesheetProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTimesheetProfileEngine DefaultRegionalTimesheetProfileEngine(Registry flags)
    {
        return new RegionalTimesheetProfileEngine(
            new List<IRegionalTimesheetProfileModifier>
            {
                new RoundingRegionalTimesheetProfileAdjuster(0.1),
                new PercentRegionalTimesheetProfileAdjuster(-3.0),
                new CapRegionalTimesheetProfileAdjuster(29687.0),
                new MinimumRegionalTimesheetProfileAdjuster(24.2),
                new FlaggedRegionalTimesheetProfileAdjuster(flags, "regional-timesheet-profile-seasonal", -0.9),
                new TieredRegionalTimesheetProfileAdjuster(
                    new List<double> { 2113.0, 2946.0 },
                    new List<double> { 0.92, 1.01 }),
            },
            new List<string>(),
            2.5,
            23606.0);
    }

    public double Compute(RegionalTimesheetProfile item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTimesheetProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
