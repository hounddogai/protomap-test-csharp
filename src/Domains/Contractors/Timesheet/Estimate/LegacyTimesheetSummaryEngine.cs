namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy timesheet summary calculation.</summary>
public interface ILegacyTimesheetSummaryFactor
{
    string Describe();
    double Adjust(LegacyTimesheetSummary item, double valueValue);
}

public sealed record TieredLegacyTimesheetSummaryAdjuster(List<double> Bounds, List<double> Rates) : ILegacyTimesheetSummaryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyTimesheetSummary item, double valueValue)
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

public sealed record CapLegacyTimesheetSummaryAdjuster(double Cap) : ILegacyTimesheetSummaryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTimesheetSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLegacyTimesheetSummaryAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTimesheetSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTimesheetSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy timesheet summary steps in order and keeps a trace of each.</summary>
public sealed class LegacyTimesheetSummaryEngine(List<ILegacyTimesheetSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTimesheetSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTimesheetSummaryEngine DefaultLegacyTimesheetSummaryEngine(Registry flags)
    {
        return new LegacyTimesheetSummaryEngine(
            new List<ILegacyTimesheetSummaryFactor>
            {
                new TieredLegacyTimesheetSummaryAdjuster(
                    new List<double> { 1469.0, 1889.0, 3604.0 },
                    new List<double> { 0.9, 1.16, 0.9 }),
                new CapLegacyTimesheetSummaryAdjuster(14924.0),
                new FlaggedLegacyTimesheetSummaryAdjuster(flags, "legacy-timesheet-summary-strict", 7.0),
            },
            new List<string>(),
            0.6,
            40982.8);
    }

    public double Run(LegacyTimesheetSummary item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTimesheetSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
