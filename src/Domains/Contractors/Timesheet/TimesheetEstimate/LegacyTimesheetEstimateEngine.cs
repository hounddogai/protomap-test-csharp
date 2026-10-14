namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy timesheet estimate calculation.</summary>
public interface ILegacyTimesheetEstimateFactor
{
    string Describe();
    double Adjust(LegacyTimesheetEstimate item, double valueValue);
}

public sealed record CapLegacyTimesheetEstimateAdjuster(double Cap) : ILegacyTimesheetEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTimesheetEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyTimesheetEstimateHoursOffsetAdjuster(double Weight) : ILegacyTimesheetEstimateFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LegacyTimesheetEstimate item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record MinimumLegacyTimesheetEstimateAdjuster(double Minimum) : ILegacyTimesheetEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTimesheetEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyTimesheetEstimateAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTimesheetEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTimesheetEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy timesheet estimate steps in order and keeps a trace of each.</summary>
public sealed class LegacyTimesheetEstimateEngine(List<ILegacyTimesheetEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTimesheetEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTimesheetEstimateEngine DefaultLegacyTimesheetEstimateEngine(Registry flags)
    {
        return new LegacyTimesheetEstimateEngine(
            new List<ILegacyTimesheetEstimateFactor>
            {
                new CapLegacyTimesheetEstimateAdjuster(2591.0),
                new LegacyTimesheetEstimateHoursOffsetAdjuster(1.94),
                new MinimumLegacyTimesheetEstimateAdjuster(29.1),
                new FlaggedLegacyTimesheetEstimateAdjuster(flags, "legacy-timesheet-estimate-tiered", 6.1),
            },
            new List<string>(),
            0.2,
            88243.9);
    }

    public double Compute(LegacyTimesheetEstimate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTimesheetEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
