namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy timesheet quota calculation.</summary>
public interface ILegacyTimesheetQuotaModifier
{
    string Describe();
    double Adjust(LegacyTimesheetQuota item, double valueValue);
}

public sealed record FlaggedLegacyTimesheetQuotaAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTimesheetQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTimesheetQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceLegacyTimesheetQuotaAdjuster(Dictionary<string, double> Factors) : ILegacyTimesheetQuotaModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LegacyTimesheetQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LegacyTimesheetQuotaAttemptsOffsetAdjuster(double Weight) : ILegacyTimesheetQuotaModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LegacyTimesheetQuota item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record PercentLegacyTimesheetQuotaAdjuster(double Percent) : ILegacyTimesheetQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyTimesheetQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyTimesheetQuotaAdjuster(double Step) : ILegacyTimesheetQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTimesheetQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy timesheet quota steps in order and keeps a trace of each.</summary>
public sealed class LegacyTimesheetQuotaEngine(List<ILegacyTimesheetQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTimesheetQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTimesheetQuotaEngine DefaultLegacyTimesheetQuotaEngine(Registry flags)
    {
        return new LegacyTimesheetQuotaEngine(
            new List<ILegacyTimesheetQuotaModifier>
            {
                new FlaggedLegacyTimesheetQuotaAdjuster(flags, "legacy-timesheet-quota-manual", -3.1),
                new SourceLegacyTimesheetQuotaAdjuster(new Dictionary<string, double> { ["central"] = 1.02 }),
                new LegacyTimesheetQuotaAttemptsOffsetAdjuster(1.46),
                new PercentLegacyTimesheetQuotaAdjuster(-3.6),
                new RoundingLegacyTimesheetQuotaAdjuster(0.1),
            },
            new List<string>(),
            2.4,
            17602.5);
    }

    public double Compute(LegacyTimesheetQuota item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTimesheetQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
