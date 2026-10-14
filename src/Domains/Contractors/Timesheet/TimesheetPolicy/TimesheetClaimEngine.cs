namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet claim calculation.</summary>
public interface ITimesheetClaimModifier
{
    string Describe();
    double Adjust(TimesheetClaim item, double valueValue);
}

public sealed record TieredTimesheetClaimAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetClaim item, double valueValue)
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

public sealed record PercentTimesheetClaimAdjuster(double Percent) : ITimesheetClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TimesheetClaimFteOffsetAdjuster(double Weight) : ITimesheetClaimModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(TimesheetClaim item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumTimesheetClaimAdjuster(double Minimum) : ITimesheetClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the timesheet claim steps in order and keeps a trace of each.</summary>
public sealed class TimesheetClaimEngine(List<ITimesheetClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetClaimEngine DefaultTimesheetClaimEngine()
    {
        return new TimesheetClaimEngine(
            new List<ITimesheetClaimModifier>
            {
                new TieredTimesheetClaimAdjuster(
                    new List<double> { 2804.0, 4467.0, 9801.0, 9804.0 },
                    new List<double> { 1.09, 0.82, 0.96, 0.99 }),
                new PercentTimesheetClaimAdjuster(-4.2),
                new TimesheetClaimFteOffsetAdjuster(0.77),
                new MinimumTimesheetClaimAdjuster(0.1),
            },
            new List<string>(),
            0.5,
            22820.6);
    }

    public double Price(TimesheetClaim item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
